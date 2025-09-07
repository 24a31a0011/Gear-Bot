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



    // PowerGearから繋がっているGearを登録するリスト
    private List<GameObject> chainGear = new List<GameObject>();

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
        if (!chainGear.Contains(gear))
        {
            chainGear.Add(gear);
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
    /// GimmcikGearをマネージャーに登録する
    /// </summary>
    public void RegisterGimmickGear(GameObject gear)
    {
        currentGimmick.Add(gear);
    }

    private void Update()
    {
        // null（Destroyされたもの）が含まれていないかチェック
        for (int i = chainGear.Count - 1; i >= 0; i--)
        {
            if (chainGear[i] == null)
            {
                Debug.Log("リスト内のオブジェクトが削除されました！");
                chainGear.RemoveAt(i);
            }
        }
    }

    public void SearchGears()
    {
        chainGear.Clear();
        for (int i = 0; i < powerGear.Count; i++)
        {
            powerGear[i].GetComponent<Gear>().Search();
        }

        // AにあってBにないもの
        var onlyInA = previewGimmcik.Except(currentGimmick).ToList();

        if (previewGimmcik.Count != 0)
        {
            foreach (var obj in onlyInA)
            {
                obj.GetComponent<GimmcikGear>().DeactivateGimmick();
            }
        }

        previewGimmcik = currentGimmick.ToList();
        ActivGimmcik();
    }

    public void ActivGimmcik()
    {
        for (int i = 0; i < currentGimmick.Count; i++)
        {
            currentGimmick[i].GetComponent<GimmcikGear>().ActivGimmick();
        }
        currentGimmick.Clear();
    }

    public void DecrementGearNumber()
    {
        for (int i = 0; i < powerGear.Count; i++)
        {
            powerGear[i].GetComponent<Gear>().DecrementGearNum();
        }
    }
}
