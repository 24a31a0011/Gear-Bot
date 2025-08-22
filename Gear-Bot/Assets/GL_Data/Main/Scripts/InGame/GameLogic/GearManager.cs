using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearManager : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static GearManager Instance { get; private set; }

    // シーン内の全ての動力源ギアを登録するリスト
    private List<PowerGear> powerGears = new List<PowerGear>();

    // 実行中のコルーチンを保持するための変数
    private Coroutine updateCoroutine;

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

    /// <summary>
    /// 動力源ギアをマネージャーに登録する
    /// </summary>
    public void RegisterPowerGear(PowerGear gear)
    {
        if (!powerGears.Contains(gear))
        {
            powerGears.Add(gear);
        }
    }

    /// <summary>
    /// 動力源ギアをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterPowerGear(PowerGear gear)
    {
        if (powerGears.Contains(gear))
        {
            powerGears.Remove(gear);
        }
    }

    // このメソッドは、待機してから実際の処理を行うコルーチンを開始します
    public void RequestGearSystemUpdate(float delay)
    {
        // もし既に更新処理が待機中なら、それをキャンセルして新しい待機を始める
        // (短時間に何度も歯車が動いた場合に、最後の動きだけを反映させるための重要な処理)
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
        }

        // 新しいコルーチンを開始し、その参照を保持する
        updateCoroutine = StartCoroutine(UpdateSystemAfterDelay(delay));
    }

    // 実際の処理を行うコルーチン
    private IEnumerator UpdateSystemAfterDelay(float delay)
    {
        // 指定された秒数だけ待機する
        yield return new WaitForSeconds(delay);

        foreach (var powerGear in powerGears)
        {
            powerGear.StartGearSearch();
        }

        // 処理が終わったので、コルーチンの参照をnullに戻す
        updateCoroutine = null;
    }
}
