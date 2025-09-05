///
/// 作成者 : グエン
///
using System.Collections.Generic;
using UnityEngine;

public class GearManager : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static GearManager Instance { get; private set; }

    // シーン内の全てのGearを登録するリスト
    private List<GameObject> gearObj = new List<GameObject>();

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
    public void RegisterClickGrid(GameObject gear)
    {
        if (!gearObj.Contains(gear))
        {
            gearObj.Add(gear);
        }
    }

    /// <summary>
    /// Gearをマネージャーから登録解除する（オブジェクトが破壊された時など）
    /// </summary>
    public void UnregisterClickGrid(GameObject gear)
    {
        if (gearObj.Contains(gear))
        {
            gearObj.Remove(gear);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
