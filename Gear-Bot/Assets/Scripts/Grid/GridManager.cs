///
/// 作成者 : グエン
///
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static GridManager Instance { get; private set; }

    [Header("プレイヤー関連のオブジェクト")]
    [SerializeField] GameObject playerObject;
    [SerializeField] GameObject pBagObject;

    [Header("荷物のオブジェクト")]
    [SerializeField] GameObject bagObject;

    [Header("ゴールのオブジェクト")]
    [SerializeField] GameObject goalObject;

    [Header("矢印のオブジェクト")]
    [SerializeField] GameObject arrowTipPrefab;
    [SerializeField] GameObject verticalPrefab;
    [SerializeField] GameObject horizontalPrefab;
    [SerializeField] GameObject cornerPrefab;

    // シーン内の全てのGridを登録するリスト
    private List<ClickGrid> clickGrids = new List<ClickGrid>();

    // プレイヤーが進むルートを登録するリスト
    private routeGrids<ClickGrid> routeGrids = new routeGrids<ClickGrid>();

    // 右クリックドラッグ中かどうか
    private bool isRightDragging = false;

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
        routeGrids.OnChanged += () => PlaceArrows();
        pBagObject.SetActive(false);
    }

    private void Update()
    {
        // 右クリックを押した瞬間
        if (Input.GetMouseButtonDown(1))
        {
            isRightDragging = true;

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
                    if (routeGrids.Count != 0)
                    {
                        // タイルが隣同士か確認
                        if (!CheckDistance(grid.transform))
                            return;

                        grid.OnDragByManager();
                    }
                    else if (routeGrids.Count == 0 && grid.transform.position.x == playerObject.transform.position.x && grid.transform.position.z == playerObject.transform.position.z)
                    {
                        grid.OnDragByManager();
                    }
                }
            }
        }

        // 右クリックを離した瞬間
        if (Input.GetMouseButtonUp(1))
        {
            isRightDragging = false;
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
        if (!routeGrids.Contains(grid))
        {
            routeGrids.Add(grid);
            grid.SetRouteNumber(routeGrids.Count);
        }
    }

    /// <summary>
    /// ルートをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterRouteGrid(ClickGrid grid)
    {
        if (routeGrids.Contains(grid))
        {
            routeGrids.Remove(grid);
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
    /// 右クリック中かどうか
    /// </summary>
    public bool GetIsRightDragging()
    {
        return isRightDragging;
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

        if (routeGrids.Count == 0) return;

        for (int i = 0; i < routeGrids.Count; i++)
        {
            GameObject arrowPrefab = null;
            Quaternion rotation = Quaternion.identity;

            if (i == routeGrids.Count - 1)
            {
                // 最後は矢印の先端
                arrowPrefab = arrowTipPrefab;

                if (i > 0)
                {
                    Vector3 dir = (routeGrids[i -1].transform.position - routeGrids[i].transform.position).normalized;
                    rotation = Quaternion.LookRotation(dir);
                }
            }
            else if(i > 0)
            {
                // 道中
                Vector3 prevDir = (routeGrids[i].transform.position - routeGrids[i - 1].transform.position).normalized;
                Vector3 nextDir = (routeGrids[i + 1].transform.position - routeGrids[i].transform.position).normalized;

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
                Vector3 dir = (routeGrids[i + 1].transform.position - routeGrids[i].transform.position).normalized;

                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
                    arrowPrefab = horizontalPrefab;
                else
                    arrowPrefab = verticalPrefab;

                rotation = Quaternion.LookRotation(dir);
            }

            if (arrowPrefab != null)
            {
                GameObject arrow = Instantiate(arrowPrefab, routeGrids[i].transform.position, rotation);
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
        if (isButtonEnabled)
        {
            isButtonEnabled = false;
            if (routeGrids.Count == 0)
            {
                Debug.LogWarning("ルートが設定されていません");
                return;
            }

            // プレイヤーがルートの最初のマスにいなければエラー
            if (playerObject.transform.position.x != routeGrids[0].transform.position.x ||
                playerObject.transform.position.z != routeGrids[0].transform.position.z)
            {
                Debug.LogWarning("ルートの最初のマスがプレイヤーの位置と合致していません");
                return;
            }

            // ルートが途中で途切れていたらエラー
            for (int i = 0; i < routeGrids.Count - 1; i++)
            {
                float distance = Vector3.Distance(routeGrids[i].transform.position, routeGrids[i + 1].transform.position);
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

        routeGrids<ClickGrid> copyRouteList = new routeGrids<ClickGrid>(routeGrids.ToList());

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
            routeGrids[copyRouteList.Count - index].ResetState();

            // プレイヤーがbagと接触したら
            if (playerObject.transform.position.x == bagObject.transform.position.x && playerObject.transform.position.z == bagObject.transform.position.z)
            {
                bagObject.SetActive(false);
                pBagObject.SetActive(true);
                activeBag = true;
            }

            yield return new WaitForSeconds(0.5f);
        }
        yield return null;

        // プレイヤーがゴールと接触したら
        if (playerObject.transform.position.x == goalObject.transform.position.x && playerObject.transform.position.z == goalObject.transform.position.z && activeBag)
        {
            SceneController.Instance.ClearScene();
        }

        routeGrids[0].ResetState();

        isButtonEnabled = true;
    }

    /// <summary>
    /// すでにあるルートの最後の位置と新しいルートの位置の絶対値が1（隣接している）かどうか
    /// </summary>
    public bool CheckDistance(Transform gridPos)
    {
        if (routeGrids.Count == 0) return false;

        return Vector3.Distance(routeGrids.Last.transform.position, gridPos.position) <= 1f;
    }

    /// <summary>
    /// 選択されたグリッドがルートの最後の地点のグリッドか否か
    /// </summary>
    public bool CheckLastRoute(ClickGrid clickGrid)
    {
        if (routeGrids == null || routeGrids.Count == 0)
            return false;

        // 最後の要素と比較
        return routeGrids.Last == clickGrid;
    }
}