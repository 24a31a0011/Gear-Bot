using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 完全統合型カメラエリアマネージャー
/// プレイヤーの位置に応じてカメラの注視点を自動的に切り替える
/// QとEキーでカメラを回転可能
/// </summary>
public class StandaloneCameraAreaManager : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    [SerializeField] private float cameraTransitionSpeed = 3f;
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Camera Rotation Settings")]
    [SerializeField] private float rotationAngle = 90f;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Area Objects")]
    [SerializeField] private Transform groundArea;
    [SerializeField] private Transform bridgeArea;
    [SerializeField] private Transform goalArea;

    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float cameraDistance = 10f;
    [SerializeField] private float cameraHeight = 5f;
    [SerializeField] private float cameraAngle = 30f;

    [Header("Area Detection")]
    [SerializeField] private float areaCheckRadius = 3f;
    [SerializeField] private float areaCheckInterval = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGizmos = true;

    // 内部状態
    private Transform playerTransform;
    private GameObject cameraTarget;
    private bool isTransitioning = false;
    private string currentAreaName = "";
    private float nextAreaCheck = 0f;

    // 回転関連
    private float currentRotationY = 0f;
    private float targetRotationY = 0f;
    private bool isRotating = false;

    // 入力関連
    private bool qKeyPressed = false;
    private bool eKeyPressed = false;

    // エリアデータ
    private readonly Dictionary<string, AreaData> areas = new Dictionary<string, AreaData>();

    // エリア情報を格納する構造体
    [System.Serializable]
    private struct AreaData
    {
        public Transform transform;
        public Vector3 center;
        public float radius;
        public string name;

        public AreaData(Transform t, Vector3 c, float r, string n)
        {
            transform = t;
            center = c;
            radius = r;
            name = n;
        }
    }

    #region Unity Lifecycle

    private void Start()
    {
        InitializeComponents();
        SetupAreas();
        CreateCameraTarget();
        SetInitialArea();
    }

    private void Update()
    {
        HandleRotationInput();

        if (Time.time >= nextAreaCheck)
        {
            CheckPlayerArea();
            nextAreaCheck = Time.time + areaCheckInterval;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // エリアの範囲を可視化
        Gizmos.color = Color.green;
        foreach (var area in areas.Values)
        {
            Gizmos.DrawWireSphere(area.center, area.radius);
        }

        // 現在のカメラターゲットを表示
        if (cameraTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cameraTarget.transform.position, 0.5f);
        }
    }

    #endregion

    #region Initialization

    /// <summary>
    /// 必要なコンポーネントの初期化
    /// </summary>
    private void InitializeComponents()
    {
        // カメラの取得
        if (targetCamera == null)
        {
            targetCamera = Camera.main ?? FindObjectOfType<Camera>();
        }

        // プレイヤーの検索
        FindPlayer();

        // エリアオブジェクトの自動検索
        FindAreaObjects();

        if (showDebugInfo)
        {
            Debug.Log($"Initialized - Camera: {targetCamera != null}, Player: {playerTransform != null}");
        }
    }

    /// <summary>
    /// プレイヤーオブジェクトの検索
    /// </summary>
    private void FindPlayer()
    {
        // タグで検索
        GameObject player = GameObject.FindWithTag("Player");

        // タグがない場合はNavMeshAgentで検索
        if (player == null)
        {
            var agent = FindObjectOfType<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                player = agent.gameObject;
            }
        }

        // それでも見つからない場合は"Robot"という名前で検索
        if (player == null)
        {
            player = GameObject.Find("Robot");
        }

        if (player != null)
        {
            playerTransform = player.transform;
            if (showDebugInfo)
            {
                Debug.Log($"Player found: {player.name}");
            }
        }
        else
        {
            Debug.LogWarning("Player not found! Please assign manually or ensure player has 'Player' tag.");
        }
    }

    /// <summary>
    /// エリアオブジェクトの自動検索
    /// </summary>
    private void FindAreaObjects()
    {
        if (groundArea == null)
        {
            var ground = GameObject.Find("Ground");
            if (ground != null) groundArea = ground.transform;
        }

        if (bridgeArea == null)
        {
            var bridge = GameObject.Find("RotatingBridge");
            if (bridge == null) bridge = GameObject.Find("Bridge");
            if (bridge != null) bridgeArea = bridge.transform;
        }

        if (goalArea == null)
        {
            var goal = GameObject.Find("GoalGround");
            if (goal == null) goal = GameObject.Find("Goal");
            if (goal != null) goalArea = goal.transform;
        }
    }

    /// <summary>
    /// エリアデータの設定
    /// </summary>
    private void SetupAreas()
    {
        areas.Clear();

        if (groundArea != null)
        {
            var center = CalculateAreaCenter(groundArea);
            var radius = CalculateAreaRadius(groundArea);
            areas.Add("Ground", new AreaData(groundArea, center, radius, "Ground"));
        }

        if (bridgeArea != null)
        {
            var center = CalculateAreaCenter(bridgeArea);
            var radius = CalculateAreaRadius(bridgeArea);
            areas.Add("Bridge", new AreaData(bridgeArea, center, radius, "Bridge"));
        }

        if (goalArea != null)
        {
            var center = CalculateAreaCenter(goalArea);
            var radius = CalculateAreaRadius(goalArea);
            areas.Add("Goal", new AreaData(goalArea, center, radius, "Goal"));
        }

        if (showDebugInfo)
        {
            Debug.Log($"Setup {areas.Count} areas");
        }
    }

    /// <summary>
    /// エリアの中心点を計算
    /// </summary>
    private Vector3 CalculateAreaCenter(Transform areaTransform)
    {
        if (areaTransform == null) return Vector3.zero;

        var renderer = areaTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.center;
        }

        var collider = areaTransform.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.center;
        }

        return areaTransform.position;
    }

    /// <summary>
    /// エリアの半径を計算
    /// </summary>
    private float CalculateAreaRadius(Transform areaTransform)
    {
        if (areaTransform == null) return areaCheckRadius;

        var renderer = areaTransform.GetComponent<Renderer>();
        if (renderer != null)
        {
            return Mathf.Max(renderer.bounds.size.x, renderer.bounds.size.z) * 0.5f;
        }

        var collider = areaTransform.GetComponent<Collider>();
        if (collider != null)
        {
            return Mathf.Max(collider.bounds.size.x, collider.bounds.size.z) * 0.5f;
        }

        return areaCheckRadius;
    }

    /// <summary>
    /// カメラターゲットオブジェクトの作成
    /// </summary>
    private void CreateCameraTarget()
    {
        cameraTarget = new GameObject("CameraTarget");
        cameraTarget.transform.SetParent(transform);
    }

    /// <summary>
    /// 初期エリアの設定
    /// </summary>
    private void SetInitialArea()
    {
        if (areas.ContainsKey("Ground"))
        {
            SetCameraToArea("Ground");
        }
        else if (areas.Count > 0)
        {
            var firstArea = "";
            foreach (var area in areas.Keys)
            {
                firstArea = area;
                break;
            }
            SetCameraToArea(firstArea);
        }
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// 回転入力の処理（90度ステップ回転）
    /// </summary>
    private void HandleRotationInput()
    {
        // キーの押下状態をチェック
        bool qKeyDown = Input.GetKeyDown(rotateLeftKey);
        bool eKeyDown = Input.GetKeyDown(rotateRightKey);

        // 回転中でない場合のみ新しい回転を受け付ける
        if (!isRotating)
        {
            if (qKeyDown)
            {
                RotateCamera(-rotationAngle);
            }
            else if (eKeyDown)
            {
                RotateCamera(rotationAngle);
            }
        }

        // スムーズ回転の処理
        if (smoothRotation && isRotating)
        {
            currentRotationY = Mathf.LerpAngle(currentRotationY, targetRotationY, rotationSpeed * Time.deltaTime);

            // 回転完了チェック
            if (Mathf.Abs(Mathf.DeltaAngle(currentRotationY, targetRotationY)) < 0.1f)
            {
                currentRotationY = targetRotationY;
                isRotating = false;
            }
        }
        else if (!smoothRotation)
        {
            currentRotationY = targetRotationY;
        }
    }

    /// <summary>
    /// カメラを指定角度回転
    /// </summary>
    private void RotateCamera(float angle)
    {
        targetRotationY += angle;
        targetRotationY = targetRotationY % 360f;
        if (targetRotationY < 0f) targetRotationY += 360f;

        if (smoothRotation)
        {
            isRotating = true;
        }
        else
        {
            currentRotationY = targetRotationY;
        }

        if (showDebugInfo)
        {
            Debug.Log($"Camera rotating to: {targetRotationY}°");
        }
    }

    #endregion

    #region Area Detection

    /// <summary>
    /// プレイヤーの現在エリアをチェック
    /// </summary>
    private void CheckPlayerArea()
    {
        if (playerTransform == null || isTransitioning) return;

        var playerPosition = playerTransform.position;
        string closestArea = "";
        float closestDistance = float.MaxValue;

        // 最も近いエリアを検索
        foreach (var area in areas.Values)
        {
            float distance = Vector3.Distance(playerPosition, area.center);

            if (distance <= area.radius && distance < closestDistance)
            {
                closestDistance = distance;
                closestArea = area.name;
            }
        }

        // エリアが変更された場合
        if (!string.IsNullOrEmpty(closestArea) && closestArea != currentAreaName)
        {
            SetCameraToArea(closestArea);
        }
    }

    #endregion

    #region Camera Control

    /// <summary>
    /// カメラを指定エリアに設定
    /// </summary>
    private void SetCameraToArea(string areaName)
    {
        if (!areas.ContainsKey(areaName) || currentAreaName == areaName) return;

        currentAreaName = areaName;
        var targetArea = areas[areaName];

        if (showDebugInfo)
        {
            Debug.Log($"Switching camera to area: {areaName}");
        }

        StartCoroutine(TransitionCameraToArea(targetArea));
    }

    /// <summary>
    /// カメラのスムーズな移動
    /// </summary>
    private IEnumerator TransitionCameraToArea(AreaData targetArea)
    {
        isTransitioning = true;

        var startPosition = cameraTarget.transform.position;
        var endPosition = targetArea.center;

        // 移動距離に基づいて時間を調整
        float distance = Vector3.Distance(startPosition, endPosition);
        float duration = Mathf.Clamp(distance / cameraTransitionSpeed, 0.5f, 3f);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            float curveValue = transitionCurve.Evaluate(t);

            cameraTarget.transform.position = Vector3.Lerp(startPosition, endPosition, curveValue);

            yield return null;
        }

        cameraTarget.transform.position = endPosition;
        isTransitioning = false;

        if (showDebugInfo)
        {
            Debug.Log($"Camera transition completed to: {targetArea.name}");
        }
    }

    /// <summary>
    /// カメラの位置を更新（LateUpdateで呼ばれる）
    /// </summary>
    private void LateUpdate()
    {
        if (targetCamera == null || cameraTarget == null) return;

        UpdateCameraPosition();
    }

    /// <summary>
    /// カメラ位置の更新（回転を考慮）
    /// </summary>
    private void UpdateCameraPosition()
    {
        var targetPosition = cameraTarget.transform.position;

        // カメラの基本角度を計算
        var angleRad = cameraAngle * Mathf.Deg2Rad;

        // Y軸回転を適用したオフセットを計算
        var rotationY = Quaternion.Euler(0, currentRotationY, 0);
        var baseOffset = new Vector3(0, cameraHeight, -cameraDistance * Mathf.Cos(angleRad));
        var rotatedOffset = rotationY * baseOffset;

        // カメラの位置を設定
        var cameraPosition = targetPosition + rotatedOffset;
        targetCamera.transform.position = cameraPosition;

        // カメラの向きを設定
        targetCamera.transform.LookAt(targetPosition);
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// 現在のエリア名を取得
    /// </summary>
    public string GetCurrentAreaName()
    {
        return currentAreaName;
    }

    /// <summary>
    /// 移動速度を設定
    /// </summary>
    public void SetTransitionSpeed(float speed)
    {
        cameraTransitionSpeed = Mathf.Max(0.1f, speed);
    }

    /// <summary>
    /// エリア検出半径を設定
    /// </summary>
    public void SetAreaCheckRadius(float radius)
    {
        areaCheckRadius = Mathf.Max(0.1f, radius);
    }

    /// <summary>
    /// 手動でエリアを切り替え
    /// </summary>
    public void SwitchToArea(string areaName)
    {
        if (areas.ContainsKey(areaName))
        {
            SetCameraToArea(areaName);
        }
    }

    /// <summary>
    /// 回転速度を設定
    /// </summary>
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = Mathf.Max(0.1f, speed);
    }

    /// <summary>
    /// 回転角度を設定
    /// </summary>
    public void SetRotationAngle(float angle)
    {
        rotationAngle = Mathf.Clamp(angle, 1f, 180f);
    }

    /// <summary>
    /// 現在の回転角度を取得
    /// </summary>
    public float GetCurrentRotation()
    {
        return currentRotationY;
    }

    /// <summary>
    /// 回転中かどうかを取得
    /// </summary>
    public bool IsRotating()
    {
        return isRotating;
    }

    /// <summary>
    /// 回転角度をリセット
    /// </summary>
    public void ResetRotation()
    {
        currentRotationY = 0f;
        targetRotationY = 0f;
        isRotating = false;
    }

    /// <summary>
    /// 指定角度に回転を設定
    /// </summary>
    public void SetRotation(float angle)
    {
        targetRotationY = angle % 360f;
        if (targetRotationY < 0f) targetRotationY += 360f;

        if (smoothRotation)
        {
            isRotating = true;
        }
        else
        {
            currentRotationY = targetRotationY;
        }
    }

    /// <summary>
    /// 手動で左に回転
    /// </summary>
    public void RotateLeft()
    {
        if (!isRotating)
        {
            RotateCamera(-rotationAngle);
        }
    }

    /// <summary>
    /// 手動で右に回転
    /// </summary>
    public void RotateRight()
    {
        if (!isRotating)
        {
            RotateCamera(rotationAngle);
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Switch to Ground")]
    public void DebugSwitchToGround()
    {
        SwitchToArea("Ground");
    }

    [ContextMenu("Switch to Bridge")]
    public void DebugSwitchToBridge()
    {
        SwitchToArea("Bridge");
    }

    [ContextMenu("Switch to Goal")]
    public void DebugSwitchToGoal()
    {
        SwitchToArea("Goal");
    }

    [ContextMenu("List Areas")]
    public void DebugListAreas()
    {
        Debug.Log($"Found {areas.Count} areas:");
        foreach (var area in areas.Values)
        {
            Debug.Log($"- {area.name}: Center={area.center}, Radius={area.radius:F1}");
        }
    }

    [ContextMenu("Rotate Left 90°")]
    public void DebugRotateLeft()
    {
        RotateLeft();
    }

    [ContextMenu("Rotate Right 90°")]
    public void DebugRotateRight()
    {
        RotateRight();
    }

    [ContextMenu("Reset Camera Rotation")]
    public void DebugResetRotation()
    {
        ResetRotation();
    }

    #endregion

    #region Cleanup

    private void OnDestroy()
    {
        if (cameraTarget != null)
        {
            DestroyImmediate(cameraTarget);
        }
    }

    #endregion
}