///
/// 作成者 : グエン
///
using System.Collections;
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
    private HashSet<GameObject> currentGimmick = new HashSet<GameObject>();
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
    /// 繋がっているGearをマネージャーに登録する
    /// </summary>
    public void RegisterChainGear(GameObject gear)
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
                chainGear.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 繋がっている歯車を探す
    /// </summary>
    public void SearchGears()
    {
        chainGear.Clear();
        for (int i = 0; i < powerGear.Count; i++)
        {
            powerGear[i].GetComponent<Gear>().Search();
        }

        // 以前のギミックと現在のギミックを比べ、違いがあればそれは途切れているとして止める
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

    public void SearchOnly()
    {
        chainGear.Clear();
        for (int i = 0; i < powerGear.Count; i++)
        {
            powerGear[i].GetComponent<Gear>().Search();
        }

        // 以前のギミックと現在のギミックを比べ、違いがあればそれは途切れているとして止める
        var onlyInA = previewGimmcik.Except(currentGimmick).ToList();

        if (previewGimmcik.Count != 0)
        {
            foreach (var obj in onlyInA)
            {
                obj.GetComponent<GimmcikGear>().DeactivateGimmick();
            }
        }

        previewGimmcik = currentGimmick.ToList();
    }

    /// <summary>
    /// ギミックを作動させる
    /// </summary>
    public void ActivGimmcik()
    {
        foreach (var obj in currentGimmick)
        {
            obj.GetComponent<GimmcikGear>().ActivGimmick();
        }
        currentGimmick.Clear();
    }

    /// <summary>
    /// 歯車の回転可能数を減らす
    /// </summary>
    public void DecrementGearNumber()
    {
        for (int i = 0; i < powerGear.Count; i++)
        {
            powerGear[i].GetComponent<Gear>().DecrementGearNum();
        }
    }

    /// <summary>
    /// 歯車の回転アニメーションを再生
    /// </summary>
    public void ActiveAnime()
    {
        for (int i = 0; i < powerGear.Count; i++)
        {
            powerGear[i].GetComponent<Animator>().SetTrigger("rote");
        }

        for (int i = 0; i < chainGear.Count; i++)
        {
            chainGear[i].GetComponent<Animator>().SetTrigger("rote");
        }

        if (previewGimmcik.Count > 0)
        {
            GridManager.Instance.SetAnimating(true);
            StartAnime();
            StartCoroutine(SearchAnimeCoroutine());
        }

        for (int i = 0; i < previewGimmcik.Count; i++)
        {
            previewGimmcik[i].GetComponent<Animator>().SetTrigger("rote");
        }
    }

    private void StartAnime()
    {
        for (int i = 0; i < previewGimmcik.Count; i++)
        {
            previewGimmcik[i].GetComponent<GimmcikGear>().GetAnimeObj().GetComponent<AnimationController>().SetTrueAnime();
        }
    }

    private IEnumerator SearchAnimeCoroutine()
    {
        // previewGimmcik の中で1つでもアニメーション中なら待つ
        bool anyAnimating = true;
        while (anyAnimating)
        {
            anyAnimating = false;
            for (int i = 0; i < previewGimmcik.Count; i++)
            {
                var animController = previewGimmcik[i].GetComponent<GimmcikGear>().GetAnimeObj().GetComponent<AnimationController>();
                if (animController.GetIsAnimating())
                {
                    anyAnimating = true;
                    break; // 1つでも animating が true ならループ続行
                }
            }

            yield return null; // 1フレーム待機
        }

        GridManager.Instance.SetAnimating(false);
        Debug.Log("start");
    }

}
