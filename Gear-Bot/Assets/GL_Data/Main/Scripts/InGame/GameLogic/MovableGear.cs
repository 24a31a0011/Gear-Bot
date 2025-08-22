using UnityEngine;

public class MovableGear : MonoBehaviour
{
    private Vector3 lastPosition;

    private float updateDelay = 0.1f; // 歯車が静止するのを待つ時間

    void Start()
    {
        // 初期位置を保存
        lastPosition = transform.position;
    }

    private void Update()
    {
        CheckIfPositionChanged();
    }

    /// <summary>
    /// 最後に記録した位置から移動したかチェックし、移動していたらGearManagerに更新をリクエスト
    /// </summary>
    private void CheckIfPositionChanged()
    {
        // ごくわずかな移動は無視する（誤差対策）
        if (Vector3.Distance(lastPosition, transform.position) > 0.01f)
        {
            Debug.Log($"{gameObject.name} の位置が変更されました。");

            // GearManagerにシステム全体の更新をリクエスト
            GearManager.Instance.RequestGearSystemUpdate(updateDelay);

            // 現在位置を新しい「最後の位置」として更新
            lastPosition = transform.position;
        }
    }
}
