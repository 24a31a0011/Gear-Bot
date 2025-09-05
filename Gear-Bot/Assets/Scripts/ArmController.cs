using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ObjectRotationController : MonoBehaviour
{
    [Header("設定")]
    [SerializeField] private Transform tipTransform; // 先端のTransform（空の場合は自身のTransformを使用）
    [SerializeField] private Transform baseTransform; // 土台のTransform（回転の中心点）
    [SerializeField] private LayerMask targetLayerMask = -1; // 対象となるオブジェクトのレイヤー
    [SerializeField] private float detectionDistance = 10f; // 検出距離
    [SerializeField] private Vector3 detectionDirection = Vector3.down; // 検出方向（真下がデフォルト）
    [SerializeField] private float liftHeight = 2f; // 持ち上げる高さ

    [Header("動作時間設定")]
    [SerializeField] private float operationDuration = 2f; // 各段階の動作時間（秒）
    [SerializeField] private float cooldownTime = 1f; // クールタイム（秒）

    [Header("電源状態")]
    [SerializeField] private bool isPowerOn = false; // 電源状態（デバッグ用表示）
    [SerializeField] private bool showPowerStatus = true; // 電源状態をログ表示するか

    [Header("UI設定")]
    [SerializeField] private Button startButton; // 開始ボタン
    [SerializeField] private Button stopButton; // 停止ボタン
    [SerializeField] private Button nextButton; // 次の状態ボタン（デバッグ用）

    private Transform targetObject; // 掴んでいるオブジェクト
    private Vector3 originalPosition; // 掴んだオブジェクトの元の位置
    private Quaternion originalRotation; // 掴んだオブジェクトの元の回転
    private Vector3 armOriginalPosition; // Armの元の位置
    private Quaternion armOriginalRotation; // Armの元の回転

    // クレーン動作用の変数
    private enum CraneState { Idle, Lifting, Rotating, Lowering, Cooldown, Paused }
    private CraneState currentState = CraneState.Idle;
    private CraneState pausedFromState = CraneState.Idle; // 一時停止前の状態

    private float operationTimer = 0f;
    private float cooldownTimer = 0f;
    private float liftStartY;
    private float liftTargetY;
    private float rotationStartAngle = 0f;
    private float rotationTargetAngle = 180f;
    private float currentRotation = 0f;
    private float pausedRotation = 0f; // 一時停止時の回転角度
    private float pausedOperationTimer = 0f; // 一時停止時の動作タイマー
    private bool isTriggerActive = false; // トリガーオブジェクトが接触中かどうか

    // 初期状態の保存
    private Vector3 armInitialPosition;
    private Quaternion armInitialRotation;
    private bool hasStoredInitialState = false;

    void Start()
    {
        if (tipTransform == null) tipTransform = transform;
        if (baseTransform == null) baseTransform = transform;

        StoreInitialState();

        // ボタンにクリックイベントを登録
        if (startButton != null)
            startButton.onClick.AddListener(StartCraneOperation);
        
        if (stopButton != null)
            stopButton.onClick.AddListener(StopCraneOperation);
        
        if (nextButton != null)
            nextButton.onClick.AddListener(NextState);

        if (showPowerStatus) Debug.Log("システム起動 - 待機状態");
    }

    private void StoreInitialState()
    {
        armInitialPosition = transform.position;
        armInitialRotation = transform.rotation;
        hasStoredInitialState = true;

        if (showPowerStatus) Debug.Log($"初期状態保存: 位置={armInitialPosition}, 回転={armInitialRotation.eulerAngles}");
    }

    void Update()
    {
        // デバッグ用：現在の状態を定期的に表示
        if (showPowerStatus && Time.frameCount % 120 == 0) // 2秒おき
        {
            Debug.Log($"現在の状態: State={currentState}, TriggerActive={isTriggerActive}");
        }

        // 自動進行する状態の処理
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
            case CraneState.Cooldown:
                UpdateCooldown();
                break;
            case CraneState.Paused:
                // 一時停止中は何もしない
                break;
        }
    }

    // デバッグ用：次の状態に手動で進める
    public void NextState()
    {
        if (currentState == CraneState.Paused || currentState == CraneState.Cooldown)
        {
            Debug.Log("一時停止中またはクールタイム中は状態変更できません");
            return;
        }

        // 現在の状態を次に進める
        switch (currentState)
        {
            case CraneState.Idle:
                StartCraneOperation();
                break;
            case CraneState.Lifting:
                StartRotation();
                break;
            case CraneState.Rotating:
                if (targetObject != null)
                    StartLowering();
                else
                    CompleteCraneOperation();
                break;
            case CraneState.Lowering:
                CompleteCraneOperation();
                break;
        }

        Debug.Log("次の状態：" + currentState);
    }

    // スタートボタンから呼び出す
    public void StartCraneOperation()
    {
        if (currentState != CraneState.Idle)
        {
            if (showPowerStatus) Debug.Log($"動作開始失敗 - 現在の状態: {currentState}");
            return;
        }

        if (showPowerStatus) Debug.Log("クレーン動作開始");

        ResetArmToInitialState();

        armOriginalPosition = transform.position;
        armOriginalRotation = transform.rotation;

        FindAndGrabObjectBelow();
        StartLifting();
    }

    // 停止ボタンから呼び出す
    public void StopCraneOperation()
    {
        if (currentState == CraneState.Idle)
        {
            if (showPowerStatus) Debug.Log("停止ボタン - 既に待機状態");
            return;
        }

        if (currentState == CraneState.Cooldown)
        {
            if (showPowerStatus) Debug.Log("停止ボタン - クールタイム中は停止できません");
            return;
        }

        PauseOperation();
        if (showPowerStatus) Debug.Log("停止ボタン - 動作一時停止");
    }

    private void ResetArmToInitialState()
    {
        if (!hasStoredInitialState) 
        {
            Debug.LogWarning("初期状態が保存されていません");
            return;
        }

        transform.position = armInitialPosition;
        transform.rotation = armInitialRotation;

        if (showPowerStatus) Debug.Log($"Arm初期状態にリセット: 位置={armInitialPosition}");
    }

    private void FindAndGrabObjectBelow()
    {
        if (tipTransform == null)
        {
            Debug.LogError("tipTransformが設定されていません。");
            return;
        }

        Vector3 tipPosition = tipTransform.position;
        Vector3 searchDirection = detectionDirection.normalized;

        RaycastHit hit;
        if (Physics.Raycast(tipPosition, searchDirection, out hit, detectionDistance, targetLayerMask))
        {
            if (hit.collider != null && hit.collider.transform != null &&
                !IsPartOfThisObject(hit.collider.transform))
            {
                targetObject = hit.collider.transform;
                originalPosition = targetObject.position;
                originalRotation = targetObject.rotation;

                if (showPowerStatus) Debug.Log($"Box発見: {targetObject.name} - 初期位置={originalPosition}");
            }
        }
        else
        {
            if (showPowerStatus) Debug.Log("Boxが見つかりませんでしたが、Armのみで動作を開始します。");
        }
    }

    private bool IsPartOfThisObject(Transform hitTransform)
    {
        if (hitTransform == transform) return true;
        Transform current = hitTransform;
        while (current.parent != null)
        {
            current = current.parent;
            if (current == transform) return true;
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
            float currentY = Mathf.Lerp(liftStartY, liftTargetY, progress);
            Vector3 newPosition = targetObject.position;
            newPosition.y = currentY;
            targetObject.position = newPosition;
        }

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
        if (baseTransform == null) return;

        Vector3 basePosition = baseTransform.position;
        Vector3 rotationAxis = Vector3.up;

        // Boxの回転処理（Boxがある場合のみ）
        if (targetObject != null)
        {
            Vector3 relativePosition = originalPosition - basePosition;
            relativePosition.y = targetObject.position.y - basePosition.y;

            Vector3 rotatedPosition = Quaternion.AngleAxis(currentRotation, rotationAxis) * relativePosition;
            targetObject.position = basePosition + rotatedPosition;
            targetObject.rotation = originalRotation * Quaternion.AngleAxis(currentRotation, rotationAxis);
        }

        // Armの回転処理（常に実行）
        Vector3 armRelativePosition = armOriginalPosition - basePosition;
        Vector3 armRotatedPosition = Quaternion.AngleAxis(currentRotation, rotationAxis) * armRelativePosition;
        transform.position = basePosition + armRotatedPosition;
        transform.rotation = armOriginalRotation * Quaternion.AngleAxis(currentRotation, rotationAxis);
    }

    private void StartLowering()
    {
        currentState = CraneState.Lowering;
        operationTimer = 0f;
        
        if (targetObject != null)
        {
            liftStartY = targetObject.position.y;
            liftTargetY = originalPosition.y;
            if (showPowerStatus) Debug.Log($"降下開始 - 動作時間: {operationDuration}秒");
        }
    }

    private void UpdateLowering()
    {
        operationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(operationTimer / operationDuration);

        if (targetObject != null)
        {
            float currentY = Mathf.Lerp(liftStartY, liftTargetY, progress);
            Vector3 newPosition = targetObject.position;
            newPosition.y = currentY;
            targetObject.position = newPosition;
        }

        if (progress >= 1f) 
        {
            CompleteCraneOperation();
        }
    }

    private void CompleteCraneOperation()
    {
        // 動作完了時にArmの初期状態を更新
        UpdateInitialState();
        
        // クールタイム開始
        StartCooldown();
        
        // デバッグ用ログ
        string message = targetObject != null ? 
            $"クレーン動作完了: Arm位置 {transform.position}, {targetObject.name} 位置 {targetObject.position}" :
            $"クレーン動作完了: Arm位置 {transform.position} (Boxなし)";
        
        if (showPowerStatus) Debug.Log(message + $" - クールタイム開始: {cooldownTime}秒");
        
        targetObject = null;
    }

    private void UpdateInitialState()
    {
        armInitialPosition = transform.position;
        armInitialRotation = transform.rotation;

        if (showPowerStatus) Debug.Log($"初期状態更新: 新しい基準位置={armInitialPosition}, 回転={armInitialRotation.eulerAngles}");
    }

    private void StartCooldown()
    {
        currentState = CraneState.Cooldown;
        cooldownTimer = cooldownTime;

        if (showPowerStatus) Debug.Log($"クールタイム開始: {cooldownTime}秒");
    }

    private void UpdateCooldown()
    {
        cooldownTimer -= Time.deltaTime;
        
        if (cooldownTimer <= 0f) 
        {
            currentState = CraneState.Idle;
            if (showPowerStatus) Debug.Log("クールタイム終了 - 待機状態");
        }
    }

    private void PauseOperation()
    {
        if (showPowerStatus) Debug.Log($"動作一時停止（状態: {currentState}）");
        
        // 現在の状態と進行状況を保存
        pausedFromState = currentState;
        pausedRotation = currentRotation;
        pausedOperationTimer = operationTimer;
        
        if (showPowerStatus) Debug.Log($"一時停止時の進行状況保存: 回転角度={pausedRotation:F1}度, タイマー={pausedOperationTimer:F2}秒");
        
        currentState = CraneState.Paused;

        // Boxを持っている場合は落下させる
        if (targetObject != null)
        {
            DropBox();
        }
    }

    private void ResumeOperation()
    {
        if (showPowerStatus) Debug.Log($"動作再開（復帰状態: {pausedFromState}）");
        
        // 一時停止前の状態に戻す
        currentState = pausedFromState;
        
        // 状態に応じて適切に復帰
        switch (pausedFromState)
        {
            case CraneState.Lifting:
                operationTimer = pausedOperationTimer;
                if (showPowerStatus) Debug.Log($"持ち上げ動作復帰: タイマー={operationTimer:F2}秒から継続");
                break;
                
            case CraneState.Rotating:
                operationTimer = pausedOperationTimer;
                currentRotation = pausedRotation;
                
                // Armを一時停止時の回転位置に復帰
                if (baseTransform != null)
                {
                    Vector3 basePosition = baseTransform.position;
                    Vector3 armRelativePosition = armOriginalPosition - basePosition;
                    Vector3 armRotatedPosition = Quaternion.AngleAxis(currentRotation, Vector3.up) * armRelativePosition;
                    transform.position = basePosition + armRotatedPosition;
                    transform.rotation = armOriginalRotation * Quaternion.AngleAxis(currentRotation, Vector3.up);
                    
                    if (showPowerStatus) Debug.Log($"回転動作復帰: {pausedRotation:F1}度から継続（タイマー={operationTimer:F2}秒）");
                }
                break;
                
            case CraneState.Lowering:
                operationTimer = pausedOperationTimer;
                if (showPowerStatus) Debug.Log($"降下動作復帰: タイマー={operationTimer:F2}秒から継続");
                break;
        }
    }

    private void DropBox()
    {
        if (targetObject == null) return;

        Vector3 boxPosition = targetObject.position;
        RaycastHit hit;

        if (Physics.Raycast(boxPosition, Vector3.down, out hit, Mathf.Infinity))
        {
            Vector3 dropPosition = hit.point;
            targetObject.position = dropPosition;
            if (showPowerStatus) Debug.Log($"Box落下: {targetObject.name} を {dropPosition} に落下");
        }
        else
        {
            Vector3 dropPosition = targetObject.position;
            dropPosition.y = originalPosition.y;
            targetObject.position = dropPosition;
            if (showPowerStatus) Debug.Log($"Box落下: {targetObject.name} を元の高さ {dropPosition} に落下");
        }

        // 回転は元に戻す
        targetObject.rotation = originalRotation;

        // Boxの参照をクリア
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
        
        // 初期位置の表示
        if (hasStoredInitialState)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(armInitialPosition, Vector3.one * 0.2f);
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
        
        // 状態を色で表示
        switch (currentState)
        {
            case CraneState.Idle:
                Gizmos.color = Color.white;
                break;
            case CraneState.Lifting:
            case CraneState.Rotating:
            case CraneState.Lowering:
                Gizmos.color = Color.yellow;
                break;
            case CraneState.Cooldown:
                Gizmos.color = Color.red;
                break;
            case CraneState.Paused:
                Gizmos.color = Color.cyan;
                break;
        }
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.4f);
    }
}