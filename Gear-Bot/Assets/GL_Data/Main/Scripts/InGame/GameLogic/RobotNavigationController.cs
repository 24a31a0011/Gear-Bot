using UnityEngine;
using UnityEngine.AI;
using System.Collections;

/// <summary>
/// 最適化されたロボットナビゲーションコントローラー
/// NavMeshAgentを使用した効率的な自動経路探索
/// </summary>
public class OptimizedRobotNavigationController : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("ゴーストモデル設定")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Color ghostColor = Color.green;

    [Header("アニメーション設定")]
    [SerializeField] private Animator robotAnimator;
    [SerializeField] private float walkAnimationSpeed = 1f;

    // キャッシュされたコンポーネント
    private NavMeshAgent navAgent;
    private Camera playerCamera;

    // 状態管理
    private GameObject targetGhost;
    private bool isMoving = false;

    // 定数
    private const float VELOCITY_THRESHOLD = 0.1f;
    private const float GHOST_ALPHA = 0.3f;
    private const float NAV_MESH_SAMPLE_DISTANCE = 5f;
    private const int TRANSPARENT_RENDER_QUEUE = 3000;

    // アニメーターパラメータID（文字列比較を避けるため）
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    #region Unity Lifecycle

    private void Start()
    {
        InitializeComponents();
        SetupNavMeshAgent();
    }

    private void Update()
    {
        HandleInput();
        UpdateMovementState();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// コンポーネントの初期化とキャッシュ
    /// </summary>
    private void InitializeComponents()
    {
        // NavMeshAgentの取得・追加
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // カメラの取得
        playerCamera = Camera.main ?? FindObjectOfType<Camera>();

        // Animatorの自動取得
        if (robotAnimator == null)
        {
            robotAnimator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// NavMeshAgentの設定
    /// </summary>
    private void SetupNavMeshAgent()
    {
        navAgent.speed = moveSpeed;
        navAgent.angularSpeed = rotationSpeed;
        navAgent.stoppingDistance = stoppingDistance;
        navAgent.autoBraking = true;
        navAgent.autoRepath = true;
    }

    #endregion

    #region Input & Movement

    /// <summary>
    /// 入力処理
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    /// <summary>
    /// マウスクリック処理
    /// </summary>
    private void HandleMouseClick()
    {
        var ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, NAV_MESH_SAMPLE_DISTANCE, NavMesh.AllAreas))
            {
                MoveToPosition(navHit.position);
            }
        }
    }

    /// <summary>
    /// 指定位置への移動開始
    /// </summary>
    public void MoveToPosition(Vector3 position)
    {
        if (navAgent.SetDestination(position))
        {
            isMoving = true;
            StartWalkAnimation();
            StartCoroutine(ShowGuideElementsWhenReady(position));
        }
    }

    /// <summary>
    /// 移動状態の更新
    /// </summary>
    private void UpdateMovementState()
    {
        if (isMoving && HasReachedDestination())
        {
            StopMovement();
        }
    }

    /// <summary>
    /// 目的地に到達したかチェック
    /// </summary>
    private bool HasReachedDestination()
    {
        return !navAgent.pathPending &&
               navAgent.remainingDistance <= navAgent.stoppingDistance &&
               navAgent.velocity.magnitude < VELOCITY_THRESHOLD;
    }

    /// <summary>
    /// 移動停止処理
    /// </summary>
    private void StopMovement()
    {
        isMoving = false;
        HideGuideElements();
        StopWalkAnimation();
    }

    #endregion

    #region Guide Elements

    /// <summary>
    /// 経路準備完了後にガイド要素を表示
    /// </summary>
    private IEnumerator ShowGuideElementsWhenReady(Vector3 targetPosition)
    {
        // 経路計算完了まで待機
        yield return new WaitUntil(() => !navAgent.pathPending && navAgent.path.corners.Length > 1);

        CreateTargetGhost(targetPosition);
    }

    /// <summary>
    /// ガイド要素の非表示
    /// </summary>
    private void HideGuideElements()
    {
        // ゴーストの削除
        if (targetGhost != null)
        {
            Destroy(targetGhost);
            targetGhost = null;
        }
    }

    /// <summary>
    /// ターゲットゴーストの作成
    /// </summary>
    private void CreateTargetGhost(Vector3 position)
    {
        // 既存のゴーストを削除
        if (targetGhost != null)
        {
            Destroy(targetGhost);
        }

        // 新しいゴーストを作成
        targetGhost = ghostPrefab != null
            ? Instantiate(ghostPrefab, position, Quaternion.identity)
            : CreateDefaultGhost(position);

        MakeGhostTransparent();
    }

    /// <summary>
    /// デフォルトゴーストの作成
    /// </summary>
    private GameObject CreateDefaultGhost(Vector3 position)
    {
        var ghost = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        ghost.transform.position = position;
        ghost.transform.localScale = transform.localScale * 1.1f;

        // 不要なコライダーを削除
        Destroy(ghost.GetComponent<Collider>());

        return ghost;
    }

    /// <summary>
    /// ゴーストを半透明化
    /// </summary>
    private void MakeGhostTransparent()
    {
        if (targetGhost == null) return;

        var renderers = targetGhost.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                SetMaterialTransparent(material);
            }
        }
    }

    /// <summary>
    /// マテリアルを透明化
    /// </summary>
    private void SetMaterialTransparent(Material material)
    {
        // Standard Shader用の透明化設定
        if (material.shader.name.Contains("Standard"))
        {
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        else
        {
            // 汎用透明化設定
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
        }

        material.renderQueue = TRANSPARENT_RENDER_QUEUE;

        var color = material.color;
        color.a = GHOST_ALPHA;
        material.color = color;
    }

    #endregion

    #region Animation

    /// <summary>
    /// 歩行アニメーション開始
    /// </summary>
    private void StartWalkAnimation()
    {
        if (robotAnimator != null)
        {
            robotAnimator.SetBool(IsWalkingHash, true);
            robotAnimator.speed = walkAnimationSpeed;
        }
    }

    /// <summary>
    /// 歩行アニメーション停止
    /// </summary>
    private void StopWalkAnimation()
    {
        if (robotAnimator != null)
        {
            robotAnimator.SetBool(IsWalkingHash, false);
        }
    }

    #endregion

    #region Public Interface

    /// <summary>
    /// 移動速度を設定
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
        if (navAgent != null)
            navAgent.speed = speed;
    }

    /// <summary>
    /// 現在移動中かどうか
    /// </summary>
    public bool IsMoving => isMoving;

    /// <summary>
    /// 現在の目的地
    /// </summary>
    public Vector3 CurrentDestination => navAgent.destination;

    #endregion
}