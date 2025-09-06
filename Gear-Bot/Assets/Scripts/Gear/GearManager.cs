///
/// 作成者 : グエン
///
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GearManager : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static GearManager Instance { get; private set; }

    // シーン内の全てのGearを登録するリスト
    private List<GameObject> gearObj = new List<GameObject>();

    // シーン内の全てのPowerGearを登録するリスト
    private List<GameObject> powerGear = new List<GameObject>();

    // 現在電源が届いているGimmcikを登録するリスト
    private List<GameObject> currentGimmick = new List<GameObject>();
    // 以前電源が届いていたGimmcikを登録するリスト
    private List<GameObject> previewGimmcik = new List<GameObject>();

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
    /// Gearをマネージャーに登録する
    /// </summary>
    public void RegisterClickGear(GameObject gear)
    {
        if (!gearObj.Contains(gear))
        {
            gearObj.Add(gear);
        }
    }

    /// <summary>
    /// Gearをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterClickGear(GameObject gear)
    {
        if (gearObj.Contains(gear))
        {
            gearObj.Remove(gear);
        }
    }

    /// <summary>
    /// PowerGearをマネージャーに登録する
    /// </summary>
    public void RegisterPowerGear(GameObject gear)
    {
        if (!powerGear.Contains(gear))
        {
            powerGear.Add(gear);
        }
    }

    /// <summary>
    /// PowerGearをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterPowerGear(GameObject gear)
    {
        if (powerGear.Contains(gear))
        {
            powerGear.Remove(gear);
        }
    }

    /// <summary>
    /// GimmcikGearをマネージャーに登録する
    /// </summary>
    public void RegisterGimmickGear(GameObject gear)
    {
        if (!currentGimmick.Contains(gear))
        {
            currentGimmick.Add(gear);
        }
    }

    /// <summary>
    /// GimmickGearをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterGimmickGear(GameObject gear)
    {
        if (currentGimmick.Contains(gear))
        {
            currentGimmick.Remove(gear);
        }
    }

    public void SearchGears()
    {
        currentGimmick.Clear();

        for (int i = 0; i < powerGear.Count; i++)
        {
            powerGear[i].GetComponent<Gear>().Search();
        }

        // AにあってBにないもの
        var onlyInA = previewGimmcik.Except(currentGimmick).ToList();

        foreach (var obj in onlyInA)
        {
            obj.GetComponent<GimmcikGear>().DeactivateGimmick();
        }

        previewGimmcik = currentGimmick.ToList();
    }

    public void ActivGimmcik()
    {
        for (int i = 0; i < currentGimmick.Count; i++)
        {
            currentGimmick[i].GetComponent<GimmcikGear>().ActivGimmick();
        }
    }
}
