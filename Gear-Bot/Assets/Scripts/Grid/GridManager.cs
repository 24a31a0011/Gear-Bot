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

    [Header("荷物のオブジェクト")]
    [SerializeField] GameObject bagObject;

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
    private List<SetGrid> clickGrids = new List<SetGrid>();

    // どの座標にGearが設置されているか登録するリスト
    private List<Transform> gearPos = new List<Transform>();

    // プレイヤーが進むルートを登録するリスト
    private routeGrids<SetGrid> routes = new routeGrids<SetGrid>();

    // 左クリックドラッグ中かどうか
    private bool isLeftDragging = false;

    bool isButtonEnabled = true;
    bool activeBag = false;

    bool isAnimating = false;

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
        if (Input.GetMouseButtonDown(0) && isButtonEnabled)
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
                var grid = result.gameObject.GetComponent<SetGrid>();
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
    /// Gearをマネージャーに登録する
    /// </summary>
    public void RegisterGear(GameObject gear)
    {
        if (!gearPos.Contains(gear.transform))
        {
            gearPos.Add(gear.transform);
            // マップに現在設置されているGearの情報を障害物として送る
            MapData.Instance.SetObstacles(gearPos);
        }
    }

    /// <summary>
    /// Gearをマネージャーから登録解除
    /// </summary>
    public void UnregisterGear(GameObject gear)
    {
        if (gearPos.Contains(gear.transform))
        {
            gearPos.Remove(gear.transform);
            // マップに現在設置されているGearの情報を障害物として送る
            MapData.Instance.SetObstacles(gearPos);
        }
    }


    /// <summary>
    /// Gridをマネージャーに登録する
    /// </summary>
    public void RegisterClickGrid(SetGrid grid)
    {
        if (!clickGrids.Contains(grid))
        {
            clickGrids.Add(grid);
        }
    }

    /// <summary>
    /// Gridをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterClickGrid(SetGrid grid)
    {
        if (clickGrids.Contains(grid))
        {
            clickGrids.Remove(grid);
        }
    }

    /// <summary>
    /// ルートをマネージャーに登録する
    /// </summary>
    public void RegisterRouteGrid(SetGrid grid)
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
    public void UnregisterRouteGrid(SetGrid grid)
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
    /// 処理中かどうか
    /// </summary>
    public bool GetEnable()
    {
        return isButtonEnabled;
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
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.ResetRoute);

        if (isButtonEnabled)
        {
            ResetState();
        }
    }

    // ボタンが押されたらプレイヤーをルート通りに動かす
    public void OnClickStartPlayerMove()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.RouteStart);

        if (isButtonEnabled)
        {
            isButtonEnabled = false;
            if (routes.Count == 0)
            {
                Debug.LogWarning("ルートが設定されていません");
                isButtonEnabled = true;
                return;
            }

            // プレイヤーがルートの最初のマスにいなければエラー
            if (playerObject.transform.position.x != routes[0].transform.position.x ||
                playerObject.transform.position.z != routes[0].transform.position.z)
            {
                Debug.LogWarning("ルートの最初のマスがプレイヤーの位置と合致していません");
                isButtonEnabled = true;
                return;
            }

            // ルートが途中で途切れていたらエラー
            for (int i = 0; i < routes.Count - 1; i++)
            {
                float distance = Vector3.Distance(routes[i].transform.position, routes[i + 1].transform.position);
                if (distance != 1)
                {
                    Debug.LogWarning("ルートが途中で途切れています");
                    isButtonEnabled = true;
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

        routeGrids<SetGrid> copyRouteList = new routeGrids<SetGrid>(routes.ToList());

        int index = copyRouteList.Count;

        bool exitLoop = false;

        for (int i = 1; i < copyRouteList.Count && !exitLoop; i++)
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

            TileData nextTile = MapData.Instance.GetTileData(targetPos);

            // 次に進む場所に障害物があればやり直し
            if (nextTile != null && (nextTile.type == TileType.PowerGear || nextTile.type == TileType.GimmickGear || nextTile.type == TileType.Obstacle || nextTile.type == TileType.Belt || nextTile.type == TileType.Arm))
            {
                Debug.LogWarning("次に進む方向に障害物があります");
                StartCoroutine(FadeSequence());
                break;
            }

            if (nextTile != null && nextTile.type == TileType.Gate && nextTile.gimmickPrefab != null)
            {
                if (MapData.Instance.GetGateTaegetPos() == playerObject.transform.position)
                {
                    if (!nextTile.gimmickPrefab.GetComponent<Bridge>().GetActiv())
                    {
                        Debug.LogWarning("ギミックが作動していません");
                        StartCoroutine(FadeSequence());
                        break;
                    }
                }
            }

            // 電源ギアから接続しているギアをすべて確認する
            GearManager.Instance.SearchGears();
            GearManager.Instance.ActiveAnime();
            GearManager.Instance.DecrementGearNumber();


            // アニメーション中なら待機
            yield return new WaitWhile(() => isAnimating);
            Debug.Log("hi");

            // --- ② 回転が終わってから移動 ---
            float distance = Vector3.Distance(startPos, targetPos);
            float elapsed = 0f;

            PlayerAnimation.Instance.NextAnime();

            while (elapsed < distance / moveSpeed)
            {
                elapsed += Time.deltaTime;
                playerObject.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / (distance / moveSpeed));
                yield return null;
            }

            PlayerAnimation.Instance.NextAnime();

            // 最後に正確にターゲットに合わせる
            playerObject.transform.position = targetPos;

            // すでに進んだ分のルートは消す
            routes[copyRouteList.Count - index].ResetState();

            TileData tile = MapData.Instance.GetTileData(playerObject.transform.position);

            // 現在地とマップ上の位置を考慮し、TileTypeごとに処理を変更
            if (tile != null && tile.type == TileType.Abyss)
            {
                Debug.LogWarning("空中です");
                StartCoroutine(FallDown(playerObject, 3f, 2f)); // (対象, 下げる量, 速度)
                StartCoroutine(FadeSequence());
                break;
            }
            else if (tile != null && tile.type == TileType.Bridge && tile.gimmickPrefab != null)
            {
                if (!tile.gimmickPrefab.GetComponent<Bridge>().GetActiv())
                {
                    Debug.LogWarning("ギミックが作動していません");
                    StartCoroutine(FallDown(playerObject, 3f, 2f)); // (対象, 下げる量, 速度)
                    StartCoroutine(FadeSequence());
                    break;
                }
            }

            // プレイヤーがbagと接触したら
            if (bagObject.transform.position.x == copyRouteList[i].transform.position.x && bagObject.transform.position.z == copyRouteList[i].transform.position.z)
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

            // プレイヤーがゴールと接触し、かつbagを持ってたら
            if (tile != null && tile.type == TileType.Goal && activeBag)
            {
                SceneController.Instance.ClearScene();
            }
            else if (i + 1 == copyRouteList.Count)
            {
                // 最終地点でゴールに接触していなかったら
                Debug.LogWarning("ゴールに触れていません");
                StartCoroutine(FadeSequence());
                break;
            }
            yield return new WaitForSeconds(0.1f);
            // ギアが壊れたそのターンにギミックを解除するため
            GearManager.Instance.SearchOnly();
            if (tile != null && tile.type == TileType.Bridge && tile.gimmickPrefab != null)
            {
                if (!tile.gimmickPrefab.GetComponent<Bridge>().GetActiv())
                {
                    Debug.LogWarning("ギミックが作動していません");
                    StartCoroutine(FallDown(playerObject, 3f, 2f)); // (対象, 下げる量, 速度)
                    StartCoroutine(FadeSequence());
                    break;
                }
            }

            yield return new WaitForSeconds(0.2f);
        }
        yield return null;

        

        routes[0].ResetState();

        isButtonEnabled = true;
    }

    private IEnumerator FallDown(GameObject obj, float fallAmount, float speed)
    {
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = new Vector3(startPos.x, startPos.y - fallAmount, startPos.z);

        float elapsed = 0f;
        float duration = 1f / speed; // 速度を補間に使う

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        obj.transform.position = targetPos;
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
    public bool CheckLastRoute(SetGrid clickGrid)
    {
        if (routes == null || routes.Count == 0)
            return false;

        // 最後の要素と比較
        return routes.Last == clickGrid;
    }

    private IEnumerator FadeSequence()
    {
        // 1. 暗転

        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.StageReset);

        yield return StartCoroutine(fadeController.FadeOut());

        // 2. 演出
        yield return new WaitForSeconds(1f); // 例: 1秒待機

        // 3. Scene再読み込み
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SetAnimating(bool value)
    {
        isAnimating = value;
    }
}