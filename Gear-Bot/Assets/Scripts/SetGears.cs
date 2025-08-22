// 榊の担当スクリプト
// 主にギアの設置と取り外しをするスクリプト
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SetGears : MonoBehaviour
{
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
        Vector3 gearpostion = new Vector3 (pos.position.x, pos.position.y, pos.position.x);
        // ギアが選ばれているかつギアが1個以上持っている時、ギアを設置する
        if (seleteGearNumber >= 0 && gearList[seleteGearNumber].gearPieces > 0)
        {
            // ギアを生成する
            GameObject gear = Instantiate(gearObject, gearpostion, Quaternion.identity);
            // 生成したギアに情報を受け渡す
            gear.GetComponent<GearDataBase>().SetGearDateBase(seleteGearNumber,
                                                            gearList[seleteGearNumber].rotatecount);
            // 置いた種類のギアの所持数を減らす
            gearList[seleteGearNumber].gearPieces -= 1;
            // テキストの数字を変更する
            gearList[seleteGearNumber].gearPiecesText.text =
                gearList[seleteGearNumber].gearPieces.ToString();
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
        gearList[number].gearPiecesText.text =
            gearList[number].gearPieces.ToString();
        // 押されたギアを消す
        Destroy(obj);
    }
}
