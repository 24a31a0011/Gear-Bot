// 榊の担当スクリプト
// 主にギアの設置と取り外しをするスクリプト
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SetGears : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static SetGears Instance { get; private set; }

    [SerializeField] private GameObject gearObject;
    // 各ギアの情報
    [System.Serializable]
    private class GearData
    {
        [Header("ギアのボタン")]
        public GameObject gearprefab;
        [Header("回転可能回数")]
        public byte rotatecount;
        [Header("所持数")]
        public sbyte gearPieces;
        [Header("所持数のテキスト")]
        public Text gearPiecesText;
    }

    // インスペクターから受け取ったギアの情報
    [SerializeField] private List<GearData> gearList = new List<GearData>();

    // 選ばれているギアの番号(リストの配列から取得する)
    private sbyte seleteGearNumber = -1;

    // ギアに表示する数字のマテリアル
    [SerializeField] private Material[] gearMaterial;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ギアの初期所持数をテキストに反映する
        for (sbyte i = 0; i < gearList.Count; i++)
        {
            GearNumTextChange(i);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// 押されたギアのボタンを取得する
    /// </summary>
    public void GetPushGearButton(int num)
    {
        // 選ばれているギアのボタンが押されたとき
        if (num == seleteGearNumber)
        {
            seleteGearNumber = -1;
        }
        // そうでなければそのギアが選ばれた状態にする
        else
        {
            seleteGearNumber = (sbyte)num;
        }
    }
    /// <summary>
    /// ギアを設置する関数
    /// </summary>
    /// <param name="pos"></param>
    public void SetGear (Transform pos)
    {
        // Transformをギアの生成に必要なVectorに変換する
        Vector3 gearpostion = new Vector3 (pos.position.x, pos.position.y, pos.position.z);
        // ギアが選ばれているかつギアが1個以上持っている時、ギアを設置する
        if (seleteGearNumber >= 0 && gearList[seleteGearNumber].gearPieces > 0)
        {
            // ギアを生成する
            GameObject gear = (GameObject)Instantiate(gearObject, gearpostion, Quaternion.identity);
            // ギアをマスの子オブジェクトにする。
            gear.transform.parent = pos.transform;
            // 生成したギアの子オブジェクトを取得
            GameObject gearChild = gear.transform.GetChild(1).gameObject;
            // 生成したギアに情報を受け渡す
            gearChild.GetComponent<GearDataBase>().SetGearDateBase(seleteGearNumber,
                                                            gearList[seleteGearNumber].rotatecount);
            // 置いた種類のギアの所持数を減らす
            gearList[seleteGearNumber].gearPieces -= 1;
            // テキストの数字を変更する
            GearNumTextChange(seleteGearNumber);
        }
    }
    /// <summary>
    /// ギアを外す関数
    /// </summary>
    public void RemoveGear(GameObject obj)
    {
        // どの種類のギアかを取得する
        sbyte number = obj.GetComponent<GearDataBase>().GetgearNumber;
        // 外した種類のギアの所持数を増やす
        gearList[number].gearPieces += 1;
        // テキストの数字を変更する
        GearNumTextChange(number);
        // 押されたギアを消す
        Destroy(obj);
    }

    /// <summary>
    /// 現在ギアを設置できる状態かを返す関数
    /// </summary>
    public bool CheckGearPlacement()
    {
        return seleteGearNumber >= 0 && gearList[seleteGearNumber].gearPieces > 0;
    }
    /// <summary>
    /// ギアの所持数のテキストを変える関数
    /// </summary>
    /// <param name="num"></param>
    void GearNumTextChange(sbyte num)
    {
        gearList[num].gearPiecesText.text =
                gearList[num].gearPieces.ToString();
    }

    public Material[] GetgearMaterialLength
    {
        get { return this.gearMaterial; }
    }

    public Material GetgearMaterial(byte num)
    {
        return this.gearMaterial[num];
    }
}
