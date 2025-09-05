///
/// 作成者 : グエン
///
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static GridManager Instance { get; private set; }

    [Header("プレイヤー関連のデータ")]
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject pBagObject;
    [SerializeField] Vector3 startPosition;

    [Header("荷物のオブジェクト")]
    [SerializeField] GameObject bagObject;

    [Header("ゴールのオブジェクト")]
    [SerializeField] GameObject goalObject;

    [Header("矢印のオブジェクト")]
    [SerializeField] GameObject arrowTipPrefab;
    [SerializeField] GameObject verticalPrefab;
    [SerializeField] GameObject horizontalPrefab;
    [SerializeField] GameObject cornerPrefab;

    [Header("移動可能回数")]
    [SerializeField] int availableMoves;
    // 残り移動可能回数
    private int remainingMoves;
    [SerializeField] Text showRemainingMoves;

    private FadeController fadeController;

    // シーン内の全てのGridを登録するリスト
    private List<ClickGrid> clickGrids = new List<ClickGrid>();

    // プレイヤーが進むルートを登録するリスト
    private routeGrids<ClickGrid> routes = new routeGrids<ClickGrid>();

    // 開始buttonを押した直前のルートの状態を保存
    private routeGrids<ClickGrid> currentRouteGrids = new routeGrids<ClickGrid>();

    // 左クリックドラッグ中かどうか
    private bool isLeftDragging = false;

    bool isButtonEnabled = true;
    bool activeBag = false;
    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        routes.OnChanged += () => PlaceArrows();
        remainingMoves = availableMoves;
        fadeController = GetComponent<FadeController>();
        if (pBagObject == null) return;
        pBagObject.SetActive(false);
    }

    private void Update()
    {
        showRemainingMoves.text = remainingMoves.ToString();

        // 左クリックを押した瞬間
        if (Input.GetMouseButtonDown(0))
        {
            isLeftDragging = true;

            // 押下した位置にあるGridも塗る
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, results);

            foreach (var result in results)
            {
                var grid = result.gameObject.GetComponent<ClickGrid>();
                if (grid != null)
                {
                    // ルートの最初か否かで処理を変更
                    if (routes.Count != 0)
                    {
                        // タイルが隣同士か確認
                        if (!CheckDistance(grid.transform))
                            return;

                        grid.OnDragByManager();
                    }
                    else if (routes.Count == 0 && grid.transform.position.x == playerObject.transform.position.x && grid.transform.position.z == playerObject.transform.position.z)
                    {
                        grid.OnDragByManager();
                    }
                }
            }
        }

        // 左クリックを離した瞬間
        if (Input.GetMouseButtonUp(0))
        {
            isLeftDragging = false;
        }
    }

    /// <summary>
    /// Gridをマネージャーに登録する
    /// </summary>
    public void RegisterClickGrid(ClickGrid grid)
    {
        if (!clickGrids.Contains(grid))
        {
            clickGrids.Add(grid);
        }
    }

    /// <summary>
    /// Gridをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterClickGrid(ClickGrid grid)
    {
        if (clickGrids.Contains(grid))
        {
            clickGrids.Remove(grid);
        }
    }

    /// <summary>
    /// ルートをマネージャーに登録する
    /// </summary>
    public void RegisterRouteGrid(ClickGrid grid)
    {
        if (!routes.Contains(grid))
        {
            routes.Add(grid);
            grid.SetRouteNumber(routes.Count);
        }
    }

    /// <summary>
    /// ルートをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterRouteGrid(ClickGrid grid)
    {
        if (routes.Contains(grid))
        {
            routes.Remove(grid);
        }
    }

    /// <summary>
    /// Stateをリセットする
    /// </summary>
    public void ResetState()
    {
        foreach (var clickGrid in clickGrids)
        {
            clickGrid.ResetState();
        }
    }

    /// <summary>
    /// 左クリック中かどうか
    /// </summary>
    public bool GetIsLeftDragging()
    {
        return isLeftDragging;
    }

    /// <summary>
    /// 現在のルートに沿って矢印を設置する
    /// </summary>
    private List<GameObject> arrowObjects = new List<GameObject>(); // 生成した矢印を保持

    public void PlaceArrows()
    {
        // 既存の矢印を削除
        foreach (var obj in arrowObjects)
            Destroy(obj);
        arrowObjects.Clear();

        if (routes.Count == 0) return;

        for (int i = 0; i < routes.Count; i++)
        {
            GameObject arrowPrefab = null;
            Quaternion rotation = Quaternion.identity;

            if (i == routes.Count - 1)
            {
                // 最後は矢印の先端
                arrowPrefab = arrowTipPrefab;

                if (i > 0)
                {
                    Vector3 dir = (routes[i -1].transform.position - routes[i].transform.position).normalized;
                    rotation = Quaternion.LookRotation(dir);
                }
            }
            else if(i > 0)
            {
                // 道中
                Vector3 prevDir = (routes[i].transform.position - routes[i - 1].transform.position).normalized;
                Vector3 nextDir = (routes[i + 1].transform.position - routes[i].transform.position).normalized;

                if (Vector3.Angle(prevDir, nextDir) > 0.1f)
                {
                    // 曲がり角
                    Vector2 prev = new Vector2(prevDir.x, prevDir.z).normalized;
                    Vector2 next = new Vector2(nextDir.x, nextDir.z).normalized;

                    // Z軸の外積に相当する値を計算
                    float cross = prev.x * next.y - prev.y * next.x;

                    if (cross > 0)
                    {
                        // 反時計回り (左に曲がる)
                        rotation = Quaternion.LookRotation(nextDir) * Quaternion.Euler(0, 180, 0);
                    }
                    else if (cross < 0)
                    {
                        // 時計回り (右に曲がる)
                        rotation = Quaternion.LookRotation(nextDir) * Quaternion.Euler(0, -90, 0);
                    }
                    
                    arrowPrefab = cornerPrefab;
                }
                else
                {
                    // 直線
                    if (Mathf.Abs(nextDir.x) > Mathf.Abs(nextDir.z))
                    {
                        // 横方向
                        arrowPrefab = horizontalPrefab;
                        rotation = Quaternion.LookRotation(nextDir);
                    }
                    else
                    {
                        // 縦方向
                        arrowPrefab = verticalPrefab;
                        rotation = Quaternion.LookRotation(nextDir);
                    }
                }
            }
            else
            {
                // ルート最初のマス
                Vector3 dir = (routes[i + 1].transform.position - routes[i].transform.position).normalized;

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
                    arrowPrefab = horizontalPrefab;
                else
                    arrowPrefab = verticalPrefab;

                rotation = Quaternion.LookRotation(dir);
            }

            if (arrowPrefab != null)
            {
                GameObject arrow = Instantiate(arrowPrefab, routes[i].transform.position, rotation);
                arrowObjects.Add(arrow);
            }
        }
    }

    // ボタンが押されたらルートを消す
    public void OnClickResetRoute()
    {
        if (isButtonEnabled)
        {
            ResetState();
        }
    }

    // ボタンが押されたらプレイヤーをルート通りに動かす
    public void OnClickStartPlayerMove()
    {
        currentRouteGrids = new routeGrids<ClickGrid>(routes.ToList());
        if (isButtonEnabled)
        {
            isButtonEnabled = false;
            if (routes.Count == 0)
            {
                Debug.LogWarning("ルートが設定されていません");
                return;
            }

            // プレイヤーがルートの最初のマスにいなければエラー
            if (playerObject.transform.position.x != routes[0].transform.position.x ||
                playerObject.transform.position.z != routes[0].transform.position.z)
            {
                Debug.LogWarning("ルートの最初のマスがプレイヤーの位置と合致していません");
                return;
            }

            // ルートが途中で途切れていたらエラー
            for (int i = 0; i < routes.Count - 1; i++)
            {
                float distance = Vector3.Distance(routes[i].transform.position, routes[i + 1].transform.position);
                if (distance != 1)
                {
                    Debug.LogWarning("ルートが途中で途切れています");
                    return;
                }
            }

            // 移動開始（コルーチン）
            StartCoroutine(MovePlayerSmoothly());
        }
    }

    private IEnumerator MovePlayerSmoothly()
    {
        float moveSpeed = 2f;     // 移動速度
        float rotateSpeed = 5f;   // 回転速度

        routeGrids<ClickGrid> copyRouteList = new routeGrids<ClickGrid>(routes.ToList());

        int index = copyRouteList.Count;

        for (int i = 1; i < copyRouteList.Count; i++)
        {
            Vector3 startPos = playerObject.transform.position;
            Vector3 targetPos = new Vector3(
                copyRouteList[i].transform.position.x,
                playerObject.transform.position.y,
                copyRouteList[i].transform.position.z
            );

            // --- ① 先に回転 ---
            Vector3 direction = (targetPos - startPos).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            while (Quaternion.Angle(playerObject.transform.rotation, targetRotation) > 0.1f)
            {
                playerObject.transform.rotation = Quaternion.Slerp(
                    playerObject.transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotateSpeed
                );
                yield return null;
            }

            // 最後に正確に回転を合わせる
            playerObject.transform.rotation = targetRotation;

            // --- ② 回転が終わってから移動 ---
            float distance = Vector3.Distance(startPos, targetPos);
            float elapsed = 0f;

            while (elapsed < distance / moveSpeed)
            {
                elapsed += Time.deltaTime;
                playerObject.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / (distance / moveSpeed));
                yield return null;
            }

            // 最後に正確にターゲットに合わせる
            playerObject.transform.position = targetPos;

            // すでに進んだ分のルートは消す
            routes[copyRouteList.Count - index].ResetState();

            // プレイヤーがbagと接触したら
            if (playerObject.transform.position.x == bagObject.transform.position.x && playerObject.transform.position.z == bagObject.transform.position.z)
            {
                bagObject.SetActive(false);
                if (pBagObject == null) break;
                pBagObject.SetActive(true);
                activeBag = true;
            }

            remainingMoves--;

            // 移動可能数より多く移動しようとした場合
            if (i+1 < copyRouteList.Count)
            {
                if (i >= availableMoves)
                {
                    Debug.LogWarning("移動可能数より多く移動しようとしています");
                    // 画面を暗転させてプレイヤーを初期位置に移動させます
                    StartCoroutine(FadeSequence());
                    break;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
        yield return null;

        // プレイヤーがゴールと接触したら
        if (goalObject != null && playerObject.transform.position.x == goalObject.transform.position.x && playerObject.transform.position.z == goalObject.transform.position.z && activeBag)
        {
            SceneController.Instance.ClearScene();
        }
        else
        {
            // 最終地点でゴールに接触していなかったら
            StartCoroutine(FadeSequence());
        }

        routes[0].ResetState();

        isButtonEnabled = true;
    }

    /// <summary>
    /// すでにあるルートの最後の位置と新しいルートの位置の絶対値が1（隣接している）かどうか
    /// </summary>
    public bool CheckDistance(Transform gridPos)
    {
        if (routes.Count == 0) return false;

        return Vector3.Distance(routes.Last.transform.position, gridPos.position) <= 1f;
    }

    /// <summary>
    /// 選択されたグリッドがルートの最後の地点のグリッドか否か
    /// </summary>
    public bool CheckLastRoute(ClickGrid clickGrid)
    {
        if (routes == null || routes.Count == 0)
            return false;

        // 最後の要素と比較
        return routes.Last == clickGrid;
    }

    private IEnumerator FadeSequence()
    {
        // 1. 暗転
        yield return StartCoroutine(fadeController.FadeOut());

        // 2. 演出
        playerObject.transform.position = startPosition;
        ResetState();
        remainingMoves = availableMoves;
        for (int i = 0; i < currentRouteGrids.Count; i++)
        {
            currentRouteGrids[i].SetOnDrag();
        }
        yield return new WaitForSeconds(1f); // 例: 2秒待機

        // 3. 明るく戻す
        yield return StartCoroutine(fadeController.FadeIn());
    }
}