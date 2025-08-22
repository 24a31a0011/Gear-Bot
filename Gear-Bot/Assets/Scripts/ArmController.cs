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

    private Transform targetObject; // 掴んでいるオブジェクト
    private Vector3 originalPosition; // 掴んだオブジェクトの元の位置
    private Quaternion originalRotation; // 掴んだオブジェクトの元の回転
    private Vector3 armOriginalPosition; // Armの元の位置
    private Quaternion armOriginalRotation; // Armの元の回転
    private bool isRotating = false; // 回転中かどうか
    private float currentRotation = 0f; // 現在の回転角度

    // クレーン動作用の変数
    private enum CraneState { Idle, Lifting, Rotating, Lowering }
    private CraneState currentState = CraneState.Idle;
    private float liftStartY; // 持ち上げ開始時のY座標
    private float liftTargetY; // 持ち上げ目標のY座標

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
        // 左クリックを検出
        if (Input.GetMouseButtonDown(0) && currentState == CraneState.Idle)
        {
            HandleMouseClick();
        }

        // クレーン動作の状態管理
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

    // デバッグ用：先端と検出範囲、土台を可視化
    void OnDrawGizmosSelected()
    {
        if (tipTransform == null) return;

        // 先端の位置
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(tipTransform.position, 0.2f);

        // 土台の位置（回転中心）
        if (baseTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(baseTransform.position, 0.3f);

            // 土台からのY軸方向を示す線
            Gizmos.DrawLine(baseTransform.position, baseTransform.position + Vector3.up * 2f);
        }

        // 検出範囲とレイの可視化
        Gizmos.color = Color.yellow;
        Vector3 tipPosition = tipTransform.position;
        Vector3 searchDirection = detectionDirection.normalized;

        // レイを描画
        Gizmos.DrawLine(tipPosition, tipPosition + searchDirection * detectionDistance);

        // 検出終点
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(tipPosition + searchDirection * detectionDistance, 0.1f);

        // 方向を示す矢印（簡易版）
        Vector3 endPoint = tipPosition + searchDirection * detectionDistance;
        Vector3 right = Vector3.Cross(searchDirection, Vector3.up) * 0.3f;
        Vector3 arrowPoint1 = endPoint - searchDirection * 0.5f + right;
        Vector3 arrowPoint2 = endPoint - searchDirection * 0.5f - right;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(endPoint, arrowPoint1);
        Gizmos.DrawLine(endPoint, arrowPoint2);

        // 回転中の場合、ArmとBoxの回転軌道を表示
        if (isRotating && baseTransform != null)
        {
            Vector3 basePos = baseTransform.position;

            // Armの回転軌道（オレンジ色）
            Gizmos.color = Color.red;
            Vector3 armStartPos = armOriginalPosition;
            float armRadius = Vector3.Distance(new Vector3(basePos.x, armStartPos.y, basePos.z),
                                             new Vector3(armStartPos.x, armStartPos.y, armStartPos.z));

            for (int i = 0; i <= 18; i++) // 10度間隔で点を描画
            {
                float angle = i * 10f;
                Vector3 armRelativePos = armOriginalPosition - basePos;
                Vector3 armRotatedPos = Quaternion.AngleAxis(angle, Vector3.up) * armRelativePos;
                Vector3 armPointOnCircle = basePos + armRotatedPos;

                Gizmos.DrawWireSphere(armPointOnCircle, 0.08f);
            }

            // Boxの回転軌道（マゼンタ色）
            if (targetObject != null)
            {
                Gizmos.color = Color.magenta;
                Vector3 boxStartPos = originalPosition;

                for (int i = 0; i <= 18; i++) // 10度間隔で点を描画
                {
                    float angle = i * 10f;
                    Vector3 boxRelativePos = originalPosition - basePos;
                    Vector3 boxRotatedPos = Quaternion.AngleAxis(angle, Vector3.up) * boxRelativePos;
                    Vector3 boxPointOnCircle = basePos + boxRotatedPos;

                    Gizmos.DrawWireSphere(boxPointOnCircle, 0.05f);
                }
            }
        }
    }
}