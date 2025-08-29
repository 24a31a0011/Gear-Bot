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
    [SerializeField] private float cooldownTime = 1f; // クールタイム（秒）

    [Header("電源状態")]
    [SerializeField] private bool isPowerOn = false; // 電源状態（デバッグ用表示）
    [SerializeField] private bool showPowerStatus = true; // 電源状態をログ表示するか

    private Transform targetObject; // 掴んでいるオブジェクト
    private Vector3 originalPosition; // 掴んだオブジェクトの元の位置
    private Quaternion originalRotation; // 掴んだオブジェクトの元の回転
    private Vector3 armOriginalPosition; // Armの元の位置
    private Quaternion armOriginalRotation; // Armの元の回転

    // クレーン動作用の変数
    private enum CraneState { Idle, Lifting, Rotating, Lowering, Cooldown, Paused }
    private CraneState currentState = CraneState.Idle;
    private CraneState pausedFromState = CraneState.Idle; // 一時停止前の状態
    private float operationTimer = 0f; // 各動作の進行時間
    private float cooldownTimer = 0f; // クールタイムの残り時間
    private float liftStartY; // 持ち上げ開始時のY座標
    private float liftTargetY; // 持ち上げ目標のY座標
    private float rotationStartAngle = 0f; // 回転開始角度
    private float rotationTargetAngle = 180f; // 回転目標角度
    private float currentRotation = 0f; // 現在の回転角度
    private float pausedRotation = 0f; // 一時停止時の回転角度
    private float pausedOperationTimer = 0f; // 一時停止時の動作タイマー
    private bool isTriggerActive = false; // トリガーオブジェクトが接触中かどうか
    private int activeTriggerCount = 0; // 接触中のトリガーオブジェクトの数

    // 初期状態の保存
    private Vector3 armInitialPosition; // Armの初期位置
    private Quaternion armInitialRotation; // Armの初期回転
    private bool hasStoredInitialState = false; // 初期状態を保存済みかどうか

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

        // Armの初期状態を保存
        StoreInitialState();
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
            Debug.Log($"現在の状態: State={currentState}, PowerOn={isPowerOn}, TriggerActive={isTriggerActive}, ActiveCount={activeTriggerCount}");
        }

        // 電源がOFFになったら動作を一時停止（ただし、トリガーが無効な場合のみ）
        if (!isPowerOn && !isTriggerActive && currentState != CraneState.Idle && currentState != CraneState.Cooldown && currentState != CraneState.Paused)
        {
            PauseOperation();
        }

        // 電源がONになったら一時停止を解除
        if (isPowerOn && isTriggerActive && currentState == CraneState.Paused)
        {
            ResumeOperation();
        }

        // クレーン動作の状態管理
        switch (currentState)
        {
            case CraneState.Idle:
                // トリガーが有効で電源ONなら動作開始
                if (isTriggerActive && isPowerOn)
                {
                    StartCraneOperation();
                }
                break;
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

    void OnTriggerStay(Collider other)
    {
        // 指定レイヤーのオブジェクトが接触中の場合
        if (IsInLayerMask(other.gameObject.layer, triggerLayerMask))
        {
            // 一時停止中なら即座に電源ONして復帰
            if (currentState == CraneState.Paused && !isPowerOn)
            {
                isPowerOn = true;
                if (showPowerStatus) Debug.Log($"一時停止中 - 電源ON（Stay）: {other.gameObject.name}");
                return;
            }

            // 通常の電源ON処理
            if (isTriggerActive && !isPowerOn && (currentState == CraneState.Idle))
            {
                isPowerOn = true;
                if (showPowerStatus) Debug.Log($"電源ON - トリガー継続接触: {other.gameObject.name}");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (showPowerStatus) Debug.Log($"OnTriggerEnter: {other.gameObject.name}, Layer: {other.gameObject.layer} ({LayerMask.LayerToName(other.gameObject.layer)})");

        if (IsInLayerMask(other.gameObject.layer, triggerLayerMask))
        {
            activeTriggerCount++;
            isTriggerActive = true;

            if (showPowerStatus) Debug.Log($"トリガー有効化: {other.gameObject.name} (アクティブ数: {activeTriggerCount})");

            // 一時停止中の場合は即座に電源ONして復帰処理
            if (currentState == CraneState.Paused)
            {
                isPowerOn = true;
                if (showPowerStatus) Debug.Log($"一時停止中にトリガー接触 - 電源ON: {other.gameObject.name}");
                return;
            }

            // 電源ON条件の詳細チェック
            if (showPowerStatus) Debug.Log($"電源ON条件チェック: isPowerOn={isPowerOn}, currentState={currentState}, 条件満足={(!isPowerOn && currentState == CraneState.Idle)}");

            if (!isPowerOn && currentState == CraneState.Idle)
            {
                isPowerOn = true;
                if (showPowerStatus) Debug.Log($"電源ON - トリガー進入: {other.gameObject.name} (アクティブ数: {activeTriggerCount})");
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (showPowerStatus) Debug.Log($"OnTriggerExit: {other.gameObject.name}, Layer: {other.gameObject.layer} ({LayerMask.LayerToName(other.gameObject.layer)})");

        if (IsInLayerMask(other.gameObject.layer, triggerLayerMask))
        {
            activeTriggerCount = Mathf.Max(0, activeTriggerCount - 1);

            // 全てのトリガーオブジェクトが離れた場合のみ電源OFF
            if (activeTriggerCount <= 0)
            {
                isTriggerActive = false;

                if (isPowerOn)
                {
                    isPowerOn = false;
                    if (showPowerStatus) Debug.Log($"電源OFF - 全トリガー離脱: {other.gameObject.name} (アクティブ数: {activeTriggerCount})");
                }
            }
            else
            {
                if (showPowerStatus) Debug.Log($"トリガー離脱: {other.gameObject.name} (残りアクティブ数: {activeTriggerCount})");
            }
        }
    }

    private bool IsInLayerMask(int layer, LayerMask layerMask)
    {
        bool result = (layerMask.value & (1 << layer)) != 0;
        if (showPowerStatus)
        {
            Debug.Log($"レイヤーマスクチェック: Layer {layer} ({LayerMask.LayerToName(layer)}) は LayerMask {layerMask} に含まれる？ → {result}");
        }
        return result;
    }

    private void StartCraneOperation()
    {
        if (showPowerStatus) Debug.Log($"動作開始処理: 現在の状態={currentState}");

        // 電源を一時的にOFFにして重複動作を防ぐ
        isPowerOn = false;
        if (showPowerStatus) Debug.Log($"動作開始 - 電源を一時的にOFF");

        // Armを初期状態にリセット
        ResetArmToInitialState();

        // Armの動作基準となる位置を現在の初期状態に設定
        armOriginalPosition = transform.position;
        armOriginalRotation = transform.rotation;

        // Boxを検索
        FindAndGrabObjectBelow();

        // 持ち上げ開始（Boxがあってもなくても）
        StartLifting();
    }

    private void ResetArmToInitialState()
    {
        if (!hasStoredInitialState)
        {
            Debug.LogWarning("初期状態が保存されていません。現在の位置を使用します。");
            return;
        }

        // Armを初期位置・回転に戻す
        transform.position = armInitialPosition;
        transform.rotation = armInitialRotation;

        if (showPowerStatus) Debug.Log($"Arm初期状態にリセット: 位置={armInitialPosition}, 回転={armInitialRotation.eulerAngles}");
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

                if (showPowerStatus) Debug.Log($"Box発見: {targetObject.name} - 初期位置={originalPosition} - クレーン動作開始");
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
        // 動作完了時にArmの初期状態を更新
        UpdateInitialState();

        // クールタイム開始
        StartCooldown();

        // デバッグ用ログ
        string message = targetObject != null ?
            $"クレーン動作完了: Arm位置 {transform.position}, {targetObject.name} 位置 {targetObject.position}" :
            $"クレーン動作完了: Arm位置 {transform.position} (Boxなし)";

        if (showPowerStatus) Debug.Log(message + $" - CT開始: {cooldownTime}秒");

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
            // クールタイム終了
            currentState = CraneState.Idle;

            // トリガーがまだ接触中なら電源ON
            if (isTriggerActive)
            {
                isPowerOn = true;
                if (showPowerStatus) Debug.Log("クールタイム終了 - 電源ON（トリガー接触中）");
            }
            else
            {
                if (showPowerStatus) Debug.Log("クールタイム終了 - 待機状態");
            }
        }
    }

    private void PauseOperation()
    {
        if (showPowerStatus) Debug.Log($"電源OFF - 動作一時停止（状態: {currentState}）");

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
        if (showPowerStatus) Debug.Log($"電源ON - 動作再開（復帰状態: {pausedFromState}）");

        // 一時停止前の状態に戻す（ただし、初期位置からは開始しない）
        currentState = pausedFromState;

        // Boxが落下してしまったので、再検索が必要
        if (pausedFromState == CraneState.Lifting || pausedFromState == CraneState.Rotating || pausedFromState == CraneState.Lowering)
        {
            // Box参照をクリアして新しく検索
            targetObject = null;

            // 新しいBoxを検索
            FindAndGrabObjectBelow();

            // 見つからなければArmのみで動作継続
            if (targetObject != null)
            {
                // 新しいBoxの情報を更新
                originalPosition = targetObject.position;
                originalRotation = targetObject.rotation;
                if (showPowerStatus) Debug.Log($"復帰時に新しいBox発見: {targetObject.name}");
            }
            else
            {
                if (showPowerStatus) Debug.Log("復帰時にBox見つからず - Armのみで動作継続");
            }

            // 一時停止前の状態に応じて適切なタイマー設定
            switch (pausedFromState)
            {
                case CraneState.Lifting:
                    // 持ち上げの途中から再開
                    if (targetObject != null)
                    {
                        liftStartY = targetObject.position.y;
                        liftTargetY = liftStartY + liftHeight;
                    }
                    break;
                case CraneState.Rotating:
                    // 回転動作を最初から開始
                    operationTimer = 0f;
                    currentRotation = 0f;
                    break;
                case CraneState.Lowering:
                    // 降下動作は新しいBoxの場合最初から
                    if (targetObject != null)
                    {
                        liftStartY = targetObject.position.y;
                        liftTargetY = originalPosition.y;
                    }
                    operationTimer = 0f;
                    break;
            }
        }

        // 電源を一時的にOFFにして重複動作を防ぐ
        isPowerOn = false;
    }

    private void DropBox()
    {
        if (targetObject == null) return;

        // Boxの現在位置から真下にレイキャスト
        Vector3 boxPosition = targetObject.position;
        RaycastHit hit;

        if (Physics.Raycast(boxPosition, Vector3.down, out hit, Mathf.Infinity))
        {
            // 地面が見つかった場合、その位置にBox配置
            Vector3 dropPosition = hit.point;
            targetObject.position = dropPosition;

            if (showPowerStatus) Debug.Log($"Box落下: {targetObject.name} を {dropPosition} に落下");
        }
        else
        {
            // 地面が見つからない場合、元の高さまで下げる
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
            Gizmos.color = isPowerOn ? Color.green : Color.blue;
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

        // 電源状態を色で表示
        if (isPowerOn)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        }
        else if (currentState == CraneState.Cooldown)
        {
            // クールタイム中は赤色で表示
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.3f);
        }
        else if (currentState == CraneState.Paused)
        {
            // 一時停止中は青色で表示
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.4f);
        }
        else if (currentState != CraneState.Idle)
        {
            // 動作中は黄色で表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.4f);
        }
    }
}