using System.Collections.Generic;
using UnityEngine;

public class Gear : MonoBehaviour
{
    [Header("レイキャスト設定")]
    [SerializeField] private float scanIntervalAngle = 15f; // レイを飛ばす角度の間隔（例: 360度を15度刻みで探索

    [Header("ギアのLayer")]
    [SerializeField] LayerMask childGearMask;   // 子ギアのLayer
    [SerializeField] LayerMask gimmickGearMask; // ギミックギアのLayer

    private HashSet<GameObject> visitedObjects = new();     // 探索済みオブジェクトを記録（無限ループ防止）

    private void Start()
    {
        GearManager.Instance.RegisterPowerGear(this.gameObject);
    }

    /// <summary>
    /// 再帰的にギアを探索してギミックに到達するまで探索を続ける
    /// </summary>
    private void SearchGear(Transform origin)
    {
        // スケールを考慮したワールド空間でのSphereColliderの半径を取得
        float maxScale = Mathf.Max(origin.lossyScale.x, origin.lossyScale.y, origin.lossyScale.z);
        float worldRadius = origin.GetComponent<SphereCollider>().radius * maxScale;

        // 半径の少し大きい値をrayの長さとする
        float rayLength = worldRadius;

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
                    GearManager.Instance.RegisterClickGear(hitObj);
                    visitedObjects.Add(hitObj);
                    SearchGear(hit.transform); // 未探索なら再帰的に探索
                }
            }
            // gimmickGearMaskでギミックを探索
            else if (Physics.Raycast(origin.position, direction, out RaycastHit gimmickHit, rayLength, gimmickGearMask))
            {
                GameObject hitObj = gimmickHit.transform.gameObject;

                // 同様に訪問済みチェック
                if (!visitedObjects.Contains(hitObj))
                {
                    GearManager.Instance.RegisterGimmickGear(hitObj);
                    visitedObjects.Add(hitObj);
                }
            }

            // レイの可視化（デバッグ用）
            Debug.DrawRay(origin.position, direction * rayLength, Color.red, 0.2f);
        }
    }

    public void Search()
    {
        SearchGear(transform);
    }
}
