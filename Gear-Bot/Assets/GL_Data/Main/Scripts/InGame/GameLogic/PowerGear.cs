using System.Collections.Generic;
using UnityEngine;

public class PowerGear : MonoBehaviour
{
    [Header("レイキャスト設定")]
    [SerializeField] float rotationSpeed = 90f; // 1秒間に回転する角度 (度数法)
    [SerializeField] private float scanIntervalAngle = 15f; // レイを飛ばす角度の間隔（例: 360度を15度刻みで探索
    float rayLength = 0.5f;    // レイの長さ

    [Header("ギアのLayer")]
    [SerializeField] LayerMask childGearMask;   // 子ギアのLayer
    [SerializeField] LayerMask gimmickGearMask; // ギミックギアのLayer

    private HashSet<GameObject> visitedObjects = new();     // 探索済みオブジェクトを記録（無限ループ防止）
    private HashSet<GameObject> animeObject = new();    // アニメーションしているオブジェクトを記録

    // 前回アクティブだったギミックのリストを保持する
    private HashSet<IGimmick> activeGimmicks = new HashSet<IGimmick>();

    void Start()
    {
        // 自身をGearManagerに登録する
        if (GearManager.Instance != null)
        {
            GearManager.Instance.RegisterPowerGear(this);
        }

        animeObject.Clear();

        // ゲーム開始時に一度、初期状態の探索を実行
        StartGearSearch();
    }

    /// <summary>
    /// ギアの探索を開始する
    /// </summary>
    public void StartGearSearch()
    {
        visitedObjects.Clear();       // 探索済みリストを初期化
        RecursiveSearch(transform);       // 探索開始（DFS）
        //ギミックの状態を更新（起動・停止）
        UpdateGimmickStates(visitedObjects);
    }

    /// <summary>
    /// 再帰的にギアを探索してギミックに到達するまで探索を続ける
    /// </summary>
    /// <param name="origin">現在探索しているギアのTransform</param>
    private void RecursiveSearch(Transform origin)
    {
        visitedObjects.Add(origin.gameObject); // 現在のギアを訪問済みに登録

        StopAnimations(animeObject);
        foreach (var item in visitedObjects)
        {
            animeObject.Add(item);
        }
        PlayAnimationsAlternating(animeObject);

        // スケールを考慮したワールド空間でのSphereColliderの半径を取得
        float maxScale = Mathf.Max(origin.lossyScale.x, origin.lossyScale.y, origin.lossyScale.z);
        float worldRadius = origin.GetComponent<SphereCollider>().radius * maxScale;

        // 半径の少し大きい値をrayの長さとする
        rayLength = worldRadius + 0.1f;

        // 360度を指定角度ずつ回転してレイを飛ばす
        for (float angle = 0f; angle < 360f; angle += scanIntervalAngle)
        {
            // 方向ベクトルを計算
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 direction = rotation * Vector3.right;

            // childGearMaskでギアを探索
            if (Physics.Raycast(origin.position, direction, out RaycastHit hit, rayLength, childGearMask))
            {
                GameObject hitObj = hit.transform.gameObject;

                // 自分自身または既に探索済みのギアは無視
                if (!visitedObjects.Contains(hitObj))
                {
                    RecursiveSearch(hit.transform); // 未探索なら再帰的に探索
                }
            }
            // gimmickGearMaskでギミックを探索
            else if (Physics.Raycast(origin.position, direction, out RaycastHit gimmickHit, rayLength, gimmickGearMask))
            {
                GameObject hitObj = gimmickHit.transform.gameObject;

                // 同様に訪問済みチェック
                if (!visitedObjects.Contains(hitObj))
                {
                    visitedObjects.Add(hitObj);
                }
            }

            // レイの可視化（デバッグ用）
            Debug.DrawRay(origin.position, direction * rayLength, Color.green, 0.2f);
        }
    }

    /// <summary>
    /// 接続情報をもとに、ギミックの起動・停止を処理する
    /// </summary>
    /// <param name="currentConnections">現在接続されているオブジェクトのリスト</param>
    private void UpdateGimmickStates(HashSet<GameObject> currentConnections)
    {
        // 今回の探索で有効なギミックを一時的に格納するセット
        var newlyActiveGimmicks = new HashSet<IGimmick>();

        // 1. 今回接続されているギミックをリストアップ
        foreach (var obj in currentConnections)
        {
            // レイヤーマスクでギミックか判定
            if (((1 << obj.layer) & gimmickGearMask) != 0)
            {
                IGimmick gimmick = obj.GetComponent<IGimmick>();
                if (gimmick != null)
                {
                    newlyActiveGimmicks.Add(gimmick);
                }
            }
        }

        // 2. 【停止処理】前回アクティブだったが、今回は非アクティブになったギミックを探して停止させる
        foreach (var oldGimmick in activeGimmicks)
        {
            if (!newlyActiveGimmicks.Contains(oldGimmick))
            {
                // このギミックは接続が切れた
                oldGimmick.Deactivate();
            }
        }

        // 3. 【起動処理】今回アクティブなギミックをすべて起動する
        foreach (var newGimmick in newlyActiveGimmicks)
        {
            newGimmick.Activate();
        }

        // 4. 最後に、今回アクティブだったギミックのリストを「前回のリスト」として保存する
        activeGimmicks = newlyActiveGimmicks;
    }

    /// <summary>
    /// HashSet内の全てのアニメーションを停止する
    /// </summary>
    /// <param name="objects">アニメーションを停止するオブジェクト達</param>
    void StopAnimations(HashSet<GameObject> objects)
    {
        foreach (GameObject obj in objects)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false; // アニメーションを停止
            }
        }

        animeObject.Clear();
    }

    /// <summary>
    /// ギアの回転アニメーションを再生する
    /// </summary>
    /// <param name="objects">アニメーションを再生するオブジェクト達</param>
    void PlayAnimationsAlternating(HashSet<GameObject> objects)
    {
        bool forward = true; // 最初は順方向

        foreach (GameObject obj in objects)
        {
            Animator animator = obj.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;         // Animator を有効化
                animator.SetFloat("Speed", forward ? 1f : -1f);
            }

            forward = !forward; // 交互に切り替え
        }
    }
}
