using UnityEngine;

public class ObjectRotationController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Transform tipTransform; // 先端のTransform（空の場合は自身のTransformを使用）
    [SerializeField] private Transform baseTransform; // 土台のTransform（回転の中心点）
    [SerializeField] private LayerMask targetLayerMask = -1; // 対象となるオブジェクトのレイヤー
    [SerializeField] private float rotationSpeed = 180f; // 回転速度（度/秒）
    [SerializeField] private float detectionDistance = 10f; // 検出距離
    [SerializeField] private bool includeChildObjects = true; // 子オブジェクトのクリックも検出するか
    [SerializeField] private Vector3 detectionDirection = Vector3.down; // 検出方向（真下がデフォルト）
    [SerializeField] private float liftHeight = 2f; // 持ち上げる高さ
    [SerializeField] private float liftSpeed = 3f; // 持ち上げ・降下の速度
    [SerializeField] private float fallSpeed = 1.0f; // 落下速度（秒/1ユニット）

    private Transform targetObject; // 掴んでいるオブジェクト
    private Vector3 originalPosition; // 掴んだオブジェクトの元の位置
    private Quaternion originalRotation; // 掴んだオブジェクトの元の回転
    private Vector3 armOriginalPosition; // Armの元の位置
    private Quaternion armOriginalRotation; // Armの元の回転
    private bool isRotating = false; // 回転中かどうか
    private float currentRotation = 0f; // 現在の回転角度

    private enum CraneState { Idle, Lifting, Rotating, Lowering }
    private CraneState currentState = CraneState.Idle;
    private float liftStartY; // 持ち上げ開始時のY座標
    private float liftTargetY; // 持ち上げ目標のY座標

    // 電源管理（インスペクターで変更可能にする）
    [SerializeField] private bool isPowerOn = true; // 電源がオンかオフか

    private bool isDropping = false; // 落下中かどうか
    private float targetDropHeight; // 落下目標のY座標

    void Start()
    {
        // 先端のTransformが指定されていない場合は、自身のTransformを使用
        if (tipTransform == null)
        {
            tipTransform = transform;
            Debug.Log("tipTransformが未設定のため、自身のTransformを使用します。");
        }

        // 土台のTransformが指定されていない場合は、自身のTransformを使用
        if (baseTransform == null)
        {
            baseTransform = transform;
            Debug.Log("baseTransformが未設定のため、自身のTransformを使用します。");
        }

        // Camera.mainの存在確認
        if (Camera.main == null)
        {
            Debug.LogWarning("Camera.mainが見つかりません。シーンのカメラに'MainCamera'タグが設定されているか確認してください。");
        }
    }

    void Update()
    {
        // 電源がオフなら、アームの動作を停止してオブジェクトを落とす
        if (!isPowerOn && targetObject != null && !isDropping)
        {
            StartDropping();
        }

        if (isDropping)
        {
            // 落下処理
            DropObjectOverTime();
        }

        // 左クリックを検出
        if (isPowerOn && Input.GetMouseButtonDown(0) && currentState == CraneState.Idle)
        {
            HandleMouseClick();
        }

        // クレーン動作の状態管理
        if (isPowerOn)
        {
            switch (currentState)
            {
                case CraneState.Lifting:
                    UpdateLifting();
                    break;
                case CraneState.Rotating:
                    UpdateRotation();
                    break;
                case CraneState.Lowering:
                    UpdateLowering();
                    break;
            }
        }
    }

    private void HandleMouseClick()
    {
        // Camera.mainが存在するかチェック
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Camera.mainが見つかりません。シーンにMainCameraタグの付いたカメラが必要です。");
            return;
        }

        // マウス位置からレイを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // レイキャストでヒットしたオブジェクトをチェック
        if (Physics.Raycast(ray, out hit))
        {
            // hit.colliderがnullでないかチェック
            if (hit.collider != null && hit.collider.transform != null)
            {
                // ヒットしたオブジェクトが自分自身か、自分の子オブジェクトかチェック
                if (IsPartOfThisObject(hit.collider.transform))
                {
                    // Boxの検出とクレーン動作開始
                    StartCraneOperation();
                }
            }
        }
    }

    private bool IsPartOfThisObject(Transform hitTransform)
    {
        // 子オブジェクトのクリック検出が無効の場合、自分自身のみチェック
        if (!includeChildObjects)
            return hitTransform == transform;

        // ヒットしたオブジェクトが自分自身の場合
        if (hitTransform == transform)
            return true;

        // 親を辿って自分自身が含まれているかチェック
        Transform current = hitTransform;
        while (current.parent != null)
        {
            current = current.parent;
            if (current == transform)
                return true;
        }

        return false;
    }

    private void StartCraneOperation()
    {
        // Armの元の状態を保存
        armOriginalPosition = transform.position;
        armOriginalRotation = transform.rotation;

        // Boxを検索
        FindAndGrabObjectBelow();

        // 持ち上げ開始（Boxがあってもなくても）
        StartLifting();
    }

    private void FindAndGrabObjectBelow()
    {
        // tipTransformが設定されているかチェック
        if (tipTransform == null)
        {
            Debug.LogError("tipTransformが設定されていません。");
            return;
        }

        // 先端の位置から指定された方向に向かってレイキャスト
        Vector3 tipPosition = tipTransform.position;
        Vector3 searchDirection = detectionDirection.normalized;

        RaycastHit hit;
        if (Physics.Raycast(tipPosition, searchDirection, out hit, detectionDistance, targetLayerMask))
        {
            // ヒットしたオブジェクトが自分自身や子オブジェクトでない場合
            if (hit.collider != null && hit.collider.transform != null &&
                !IsPartOfThisObject(hit.collider.transform))
            {
                targetObject = hit.collider.transform;
                originalPosition = targetObject.position;
                originalRotation = targetObject.rotation;

                Debug.Log($"Box発見: {targetObject.name} - クレーン動作開始");
            }
            else
            {
                Debug.Log("ヒットしたオブジェクトは自分自身または子オブジェクトです。");
            }
        }
        else
        {
            Debug.Log($"Boxが見つかりませんでしたが、Armのみで動作を開始します。");
        }
    }

    private void StartLifting()
    {
        currentState = CraneState.Lifting;

        if (targetObject != null)
        {
            liftStartY = targetObject.position.y;
            liftTargetY = liftStartY + liftHeight;
            Debug.Log("持ち上げ開始（Box有り）");
        }
        else
        {
            // Boxがない場合でも持ち上げ動作のタイミングを作る
            liftStartY = 0f;
            liftTargetY = liftHeight;
            Debug.Log("持ち上げ開始（Boxなし）");
        }
    }

    private void UpdateLifting()
    {
        if (targetObject != null)
        {
            // Boxの持ち上げ処理
            float newY = Mathf.MoveTowards(targetObject.position.y, liftTargetY, liftSpeed * Time.deltaTime);
            Vector3 newPosition = targetObject.position;
            newPosition.y = newY;
            targetObject.position = newPosition;

            // 持ち上げ完了チェック
            if (Mathf.Approximately(newY, liftTargetY))
            {
                StartRotation();
            }
        }
        else
        {
            // Boxがない場合は少し待ってから回転開始
            liftStartY += liftSpeed * Time.deltaTime;
            if (liftStartY >= liftTargetY)
            {
                StartRotation();
            }
        }
    }

    private void StartRotation()
    {
        currentState = CraneState.Rotating;
        isRotating = true;
        currentRotation = 0f;

        Debug.Log($"回転開始");
    }

    private void UpdateRotation()
    {
        // 回転角度を更新
        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        currentRotation += rotationThisFrame;

        // 180度回転したかチェック
        if (currentRotation >= 180f)
        {
            currentRotation = 180f;
            CompleteRotation();
        }

        // オブジェクトを土台の周りで回転させる
        RotateObjectAroundBase();
    }

    private void RotateObjectAroundBase()
    {
        // 土台の存在確認
        if (baseTransform == null)
        {
            Debug.LogError("baseTransformが設定されていません。");
            return;
        }

        Vector3 basePosition = baseTransform.position;
        Vector3 rotationAxis = Vector3.up; // Y軸を回転軸に設定

        // Boxの回転処理（Boxがある場合のみ）
        if (targetObject != null)
        {
            // 元の位置から土台への相対位置
            Vector3 relativePosition = originalPosition - basePosition;
            relativePosition.y = targetObject.position.y - basePosition.y; // 現在の高さを維持

            // 現在の回転角度でY軸周りに回転
            Vector3 rotatedPosition = Quaternion.AngleAxis(currentRotation, rotationAxis) * relativePosition;

            // 新しい位置を設定
            targetObject.position = basePosition + rotatedPosition;

            // オブジェクト自体もY軸周りに回転させる
            targetObject.rotation = originalRotation * Quaternion.AngleAxis(currentRotation, rotationAxis);
        }

        // Armの回転処理（常に実行）
        // Armの元の位置から土台への相対位置
        Vector3 armRelativePosition = armOriginalPosition - basePosition;

        // 現在の回転角度でY軸周りに回転
        Vector3 armRotatedPosition = Quaternion.AngleAxis(currentRotation, rotationAxis) * armRelativePosition;

        // Armの新しい位置を設定
        transform.position = basePosition + armRotatedPosition;

        // Arm自体もY軸周りに回転させる
        transform.rotation = armOriginalRotation * Quaternion.AngleAxis(currentRotation, rotationAxis);
    }

    private void CompleteRotation()
    {
        isRotating = false;

        if (targetObject != null)
        {
            // 降下開始
            StartLowering();
        }
        else
        {
            // Boxがない場合はそのまま完了
            CompleteCraneOperation();
        }
    }

    private void StartLowering()
    {
        currentState = CraneState.Lowering;
        liftTargetY = originalPosition.y; // 元の高さに戻す

        Debug.Log("降下開始");
    }

    private void UpdateLowering()
    {
        if (targetObject != null)
        {
            // Boxの降下処理
            float newY = Mathf.MoveTowards(targetObject.position.y, liftTargetY, liftSpeed * Time.deltaTime);
            Vector3 newPosition = targetObject.position;
            newPosition.y = newY;
            targetObject.position = newPosition;

            // 降下完了チェック
            if (Mathf.Approximately(newY, liftTargetY))
            {
                CompleteCraneOperation();
            }
        }
    }

    private void CompleteCraneOperation()
    {
        currentState = CraneState.Idle;

        // デバッグ用ログ
        string message = targetObject != null ?
            $"クレーン動作完了: Arm位置 {transform.position}, {targetObject.name} 位置 {targetObject.position}" :
            $"クレーン動作完了: Arm位置 {transform.position} (Boxなし)";

        Debug.Log(message);

        // 必要に応じてここで追加の処理（アニメーション、エフェクト等）

        targetObject = null;
    }

    // 落下処理
    private void StartDropping()
    {
        // 落下開始時の目標高さを設定（現在のY座標から1.0f下に設定）
        targetDropHeight = targetObject.position.y - 1.0f;
        isDropping = true;
        Debug.Log("落下開始");
    }

    private void DropObjectOverTime()
    {
        if (targetObject != null)
        {
            // 現在のY座標から目標Y座標へ徐々に移動
            float newY = Mathf.MoveTowards(targetObject.position.y, targetDropHeight, fallSpeed * Time.deltaTime);
            Vector3 newPosition = targetObject.position;
            newPosition.y = newY;
            targetObject.position = newPosition;

            // 落下完了チェック
            if (Mathf.Approximately(newY, targetDropHeight))
            {
                isDropping = false;
                Debug.Log("オブジェクトの落下完了");
                CompleteCraneOperation();
            }
        }
    }
}
