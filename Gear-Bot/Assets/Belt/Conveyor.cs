using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class ConveyorBelt : MonoBehaviour
{
    [Header("グリッド移動設定")]
    public Vector3 moveDirection = Vector3.forward;  // コンベアの移動方向
    public float gridSize = 1f;                      // 1マスのサイズ
    public float slidespeed = 0.5f;                  // 移動にかかる時間(秒)

    [Header("検出エリア")]
    public float detectionHeight = 0.5f;             // 検出範囲の高さオフセット
    public float detectionWidth = 1f;                // 検出範囲の横幅（X方向）
    public float detectionLength = 2f;               // 検出範囲の奥行き（Z方向）

    [Header("レイヤー/タグフィルタ")]
    public LayerMask affectedLayers = -1;            // 移動対象となるレイヤー

    private bool moveOnce = false;                    // 一度だけ移動処理をするフラグ
    private readonly Collider[] overlapBuffer = new Collider[32];  // OverlapBox用バッファ
    private readonly HashSet<Transform> movingObjects = new HashSet<Transform>();  // 現在移動中のオブジェクト管理

    // キャッシュした値（計算を減らすため）
    private Vector3 cachedMoveOffset;
    private Vector3 cachedHalfExtents;
    private float cachedInvGridSize;
    private float cachedGridHalf;
    private bool cacheValid = false;

    // 一時リスト・バッファ（GC対策）
    private readonly List<Collider> tempColliderList = new List<Collider>(32);
    private readonly Collider[] pushCheckBuffer = new Collider[16];

    [SerializeField] Animator animator;

    private void Start()
    {
        UpdateCache();  // 初回キャッシュ更新
    }

    private void OnValidate()
    {
        // インスペクターで値を変更したらキャッシュ無効化
        cacheValid = false;
    }

    private void Update()
    {
        if (moveOnce)
        {
            moveOnce = false;
            MoveOneGrid();  // 1マス分の移動処理を呼ぶ
        }

        if (!IsMoving())
        {
            GetComponent<AnimationController>().SetFalseAnime();
        }
    }

    /// <summary>
    /// キャッシュを更新（頻繁に使う値を事前計算）
    /// </summary>
    private void UpdateCache()
    {
        cachedMoveOffset = moveDirection.normalized * gridSize;  // 移動ベクトル（正規化済み×マスサイズ）
        cachedHalfExtents = new Vector3(detectionWidth * 0.5f, 0.1f, detectionLength * 0.5f);  // OverlapBox用の半サイズ
        cachedInvGridSize = 1f / gridSize;  // グリッドの逆数（スナップ計算用）
        cachedGridHalf = gridSize * 0.5f;   // グリッドの半分（スナップ計算用）
        cacheValid = true;
    }

    /// <summary>
    /// 検出範囲内の荷物を1マス分スライドさせる
    /// </summary>
    private void MoveOneGrid()
    {
        if (!cacheValid) UpdateCache();

        // 検出範囲の中心位置（自身の位置+高さオフセット）
        Vector3 center = transform.position + Vector3.up * detectionHeight;

        // OverlapBoxで検出、結果をoverlapBufferに格納
        // 回転を無視して検出したい場合
        int count = Physics.OverlapBoxNonAlloc(center, cachedHalfExtents, overlapBuffer, Quaternion.identity, affectedLayers);

        tempColliderList.Clear();

        // 有効な対象（荷物）だけリストに抽出
        for (int i = 0; i < count; i++)
        {
            if (IsValidTarget(overlapBuffer[i]))
                tempColliderList.Add(overlapBuffer[i]);
        }

        // 移動方向の奥（進行方向）から手前へ向かってソート（押し出し順序確保）
        tempColliderList.Sort((a, b) =>
        {
            float aDist = Vector3.Dot(a.transform.position, moveDirection);
            float bDist = Vector3.Dot(b.transform.position, moveDirection);
            return bDist.CompareTo(aDist);
        });

        // ソートした順に押し出し処理を試みる
        for (int i = 0; i < tempColliderList.Count; i++)
        {
            Transform obj = tempColliderList[i].transform;
            TrySlideObject(obj, cachedMoveOffset);
        }
    }

    /// <summary>
    /// 指定オブジェクトを押し出す（再帰的に押し出し先もチェック）
    /// </summary>
    /// <param name="obj">移動対象</param>
    /// <param name="offset">移動量（1マス分）</param>
    /// <returns>移動成功したらtrue</returns>
    private bool TrySlideObject(Transform obj, Vector3 offset)
    {
        // すでに移動中なら処理しない
        if (movingObjects.Contains(obj)) return false;

        // 移動先をグリッドにスナップ
        Vector3 targetPos = SnapToGrid(obj.position + offset);

        // 移動先に他の荷物がいないかOverlapBoxで確認
        int hitCount = Physics.OverlapBoxNonAlloc(targetPos, Vector3.one * 0.3f, pushCheckBuffer, Quaternion.identity, affectedLayers);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = pushCheckBuffer[i];

            // 無効なターゲット、または自分自身は無視
            if (!IsValidTarget(hit) || hit.transform == obj) continue;

            // 他の荷物を押せなければ移動不可
            if (!TrySlideObject(hit.transform, offset))
                return false;
        }

        // 押し出し可能なので滑らかに移動開始
        StartCoroutine(SlideObject(obj, targetPos));
        return true;
    }

    /// <summary>
    /// 指定オブジェクトを滑らかに目的地まで移動させるコルーチン
    /// </summary>
    private IEnumerator SlideObject(Transform obj, Vector3 targetPos)
    {
        // アニメーション再生開始
        if (!animator.GetBool("IsMoving"))
            animator.SetBool("IsMoving", true);

        movingObjects.Add(obj);

        Vector3 startPos = obj.position;
        float invDuration = 1f / slidespeed;

        for (float elapsed = 0f; elapsed < slidespeed; elapsed += Time.deltaTime)
        {
            if (obj == null) break;

            float t = elapsed * invDuration;
            obj.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        if (obj != null)
        {
            obj.position = targetPos;
        }

        movingObjects.Remove(obj);

        // 最後の1つの荷物が移動完了したらアニメーション停止
        if (movingObjects.Count == 0)
            animator.SetBool("IsMoving", false);
    }


    /// <summary>
    /// 位置をグリッドにスナップ（中央揃え）
    /// </summary>
    private Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x * cachedInvGridSize) * gridSize,
            pos.y,
            Mathf.Round(pos.z * cachedInvGridSize) * gridSize
        );
    }


    /// <summary>
    /// 移動対象かどうか判定（対象レイヤーかつ"Luggage"タグ）
    /// </summary>
    private bool IsValidTarget(Collider col)
    {
        return ((1 << col.gameObject.layer) & affectedLayers) != 0;
    }

    /// <summary>
    /// 外部から移動処理をONにする
    /// </summary>
    public void TurnOn()
    {
        moveOnce = true;
    }

    /// <summary>
    /// 現在何かが移動中かどうかを返す
    /// </summary>
    public bool IsMoving()
    {
        return movingObjects.Count > 0;
    }



    /// <summary>
    /// ギズモで検出範囲と移動方向を可視化
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!cacheValid) UpdateCache();

        Gizmos.color = Color.green;

        Vector3 center = transform.position + Vector3.up * detectionHeight;

        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);

        // 検出範囲をワイヤーフレームで描画
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(detectionWidth, 0.2f, detectionLength));

        Gizmos.matrix = oldMatrix;

        Gizmos.color = Color.blue;

        // 移動方向の矢印描画
        Vector3 start = transform.position;
        Vector3 end = start + cachedMoveOffset.normalized * 2f;
        Gizmos.DrawLine(start, end);

        Vector3 right = Vector3.Cross(cachedMoveOffset.normalized, Vector3.up) * 0.2f;
        Vector3 arrowBase = end - cachedMoveOffset.normalized * 0.3f;
        Gizmos.DrawLine(end, arrowBase + right);
        Gizmos.DrawLine(end, arrowBase - right);
    }
}
