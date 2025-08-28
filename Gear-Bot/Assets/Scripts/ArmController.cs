using UnityEngine;

public class ObjectRotationController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Transform tipTransform; // 先端のTransform（空の場合は自身のTransformを使用）
    [SerializeField] private Transform baseTransform; // 土台のTransform（回転の中心点）
    [SerializeField] private LayerMask targetLayerMask = -1; // 対象となるオブジェクトのレイヤー
    [SerializeField] private LayerMask triggerLayerMask = -1; // 電源ONトリガーとなるレイヤー
    [SerializeField] private float detectionDistance = 10f; // 検出距離
    [SerializeField] private Vector3 detectionDirection = Vector3.down; // 検出方向（真下がデフォルト）
    [SerializeField] private float liftHeight = 2f; // 持ち上げる高さ

    [Header("動作時間設定")]
    [SerializeField] private float operationDuration = 2f; // 各段階の動作時間（秒）

    [Header("電源状態")]
    [SerializeField] private bool isPowerOn = false; // 電源状態（デバッグ用表示）
    [SerializeField] private bool showPowerStatus = true; // 電源状態をログ表示するか

    private Transform targetObject; // 掴んでいるオブジェクト
    private Vector3 originalPosition; // 掴んだオブジェクトの元の位置
    private Quaternion originalRotation; // 掴んだオブジェクトの元の回転
    private Vector3 armOriginalPosition; // Armの元の位置
    private Quaternion armOriginalRotation; // Armの元の回転

    // クレーン動作用の変数
    private enum CraneState { Idle, Lifting, Rotating, Lowering }
    private CraneState currentState = CraneState.Idle;
    private float operationTimer = 0f; // 各動作の進行時間
    private float liftStartY; // 持ち上げ開始時のY座標
    private float liftTargetY; // 持ち上げ目標のY座標
    private float rotationStartAngle = 0f; // 回転開始角度
    private float rotationTargetAngle = 180f; // 回転目標角度
    private float currentRotation = 0f; // 現在の回転角度

    void Start()
    {
        // 先端のTransformが指定されていない場合は、自身のTransformを使用
        if (tipTransform == null)
        {
            tipTransform = transform;
            if (showPowerStatus) Debug.Log("tipTransformが未設定のため、自身のTransformを使用します。");
        }

        // 土台のTransformが指定されていない場合は、自身のTransformを使用
        if (baseTransform == null)
        {
            baseTransform = transform;
            if (showPowerStatus) Debug.Log("baseTransformが未設定のため、自身のTransformを使用します。");
        }

        // 電源状態を初期化
        isPowerOn = false;
        if (showPowerStatus) Debug.Log("システム起動 - 電源OFF状態");
    }

    void Update()
    {
        // 電源がONになったら即座にクレーン動作開始
        if (isPowerOn && currentState == CraneState.Idle)
        {
            StartCraneOperation();
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

    void OnTriggerEnter(Collider other)
    {
        // 指定レイヤーのオブジェクトが触れたら電源ON
        if (IsInLayerMask(other.gameObject.layer, triggerLayerMask))
        {
            if (!isPowerOn)
            {
                isPowerOn = true;
                if (showPowerStatus) Debug.Log($"電源ON - トリガー: {other.gameObject.name}");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // 指定レイヤーのオブジェクトが離れたら電源OFF（動作中でなければ）
        if (IsInLayerMask(other.gameObject.layer, triggerLayerMask))
        {
            if (isPowerOn && currentState == CraneState.Idle)
            {
                isPowerOn = false;
                if (showPowerStatus) Debug.Log($"電源OFF - トリガー離脱: {other.gameObject.name}");
            }
        }
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        return (layerMask.value & (1 << layer)) != 0;
    }

    private void StartCraneOperation()
    {
        // 電源を一時的にOFFにして重複動作を防ぐ
        isPowerOn = false;

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

                if (showPowerStatus) Debug.Log($"Box発見: {targetObject.name} - クレーン動作開始");
            }
        }
        else
        {
            if (showPowerStatus) Debug.Log($"Boxが見つかりませんでしたが、Armのみで動作を開始します。");
        }
    }

    private bool IsPartOfThisObject(Transform hitTransform)
    {
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

    private void StartLifting()
    {
        currentState = CraneState.Lifting;
        operationTimer = 0f;

        if (targetObject != null)
        {
            liftStartY = targetObject.position.y;
            liftTargetY = liftStartY + liftHeight;
            if (showPowerStatus) Debug.Log($"持ち上げ開始（Box有り） - 動作時間: {operationDuration}秒");
        }
        else
        {
            liftStartY = 0f;
            liftTargetY = liftHeight;
            if (showPowerStatus) Debug.Log($"持ち上げ開始（Boxなし） - 動作時間: {operationDuration}秒");
        }
    }

    private void UpdateLifting()
    {
        operationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(operationTimer / operationDuration);

        if (targetObject != null)
        {
            // Boxの持ち上げ処理（滑らかな補間）
            float currentY = Mathf.Lerp(liftStartY, liftTargetY, progress);
            Vector3 newPosition = targetObject.position;
            newPosition.y = currentY;
            targetObject.position = newPosition;
        }

        // 持ち上げ完了チェック
        if (progress >= 1f)
        {
            StartRotation();
        }
    }

    private void StartRotation()
    {
        currentState = CraneState.Rotating;
        operationTimer = 0f;
        rotationStartAngle = 0f;
        currentRotation = 0f;

        if (showPowerStatus) Debug.Log($"回転開始 - 動作時間: {operationDuration}秒");
    }

    private void UpdateRotation()
    {
        operationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(operationTimer / operationDuration);

        // 回転角度を滑らかに補間
        currentRotation = Mathf.Lerp(rotationStartAngle, rotationTargetAngle, progress);

        // オブジェクトを土台の周りで回転させる
        RotateObjectAroundBase();

        // 回転完了チェック
        if (progress >= 1f)
        {
            if (targetObject != null)
            {
                StartLowering();
            }
            else
            {
                CompleteCraneOperation();
            }
        }
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

    private void StartLowering()
    {
        currentState = CraneState.Lowering;
        operationTimer = 0f;
        liftStartY = targetObject.position.y;
        liftTargetY = originalPosition.y; // 元の高さに戻す

        if (showPowerStatus) Debug.Log($"降下開始 - 動作時間: {operationDuration}秒");
    }

    private void UpdateLowering()
    {
        operationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(operationTimer / operationDuration);

        if (targetObject != null)
        {
            // Boxの降下処理（滑らかな補間）
            float currentY = Mathf.Lerp(liftStartY, liftTargetY, progress);
            Vector3 newPosition = targetObject.position;
            newPosition.y = currentY;
            targetObject.position = newPosition;
        }

        // 降下完了チェック
        if (progress >= 1f)
        {
            CompleteCraneOperation();
        }
    }

    private void CompleteCraneOperation()
    {
        currentState = CraneState.Idle;

        // デバッグ用ログ
        string message = targetObject != null ?
            $"クレーン動作完了: Arm位置 {transform.position}, {targetObject.name} 位置 {targetObject.position}" :
            $"クレーン動作完了: Arm位置 {transform.position} (Boxなし)";

        if (showPowerStatus) Debug.Log(message);

        // 電源を再びOFFにする
        isPowerOn = false;

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
            Gizmos.color = isPowerOn ? Color.green : Color.blue;
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

        // 電源状態を色で表示
        if (isPowerOn)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
    }
}