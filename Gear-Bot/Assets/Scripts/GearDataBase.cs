// 榊の担当スクリプト
// 生成したギアに情報を持たり、マテリアルの管理をするスクリプト
using UnityEngine;
using UnityEngine.UI;

public class GearDataBase : MonoBehaviour
{
    // どのボタンのギアかを格納する変数
    private sbyte gearNumber;
    // 生成したギアの回転可能回数を格納する変数
    private byte gearRotateCount;
    // 子オブジェクトを格納する変数
    private GameObject gearChild;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gearRotateCount <= 0)
        {
            GridManager.Instance.UnregisterGear(transform.parent.parent.gameObject);
            // 0.5秒後に自分を破壊
            Destroy(transform.parent.gameObject, 1f);
        }
    }
    /// <summary>
    /// ギアに情報を受け渡す関数
    /// </summary>
    public void SetGearDateBase(sbyte number, byte count)
    {
        GetgearNumber = number;
        gearRotateCount = count;
        Debug.Log(this.gearNumber + " : " + this.gearRotateCount);
        RotateMaterialChange();
    }
    /// <summary>
    /// そのギアの回転可能回数を減らす関数
    /// </summary>
    public void RotatingGear()
    {
        gearRotateCount -= 1;
        RotateMaterialChange();
    }
    /// <summary>
    /// ギアの数字のマテリアルを変える関数
    /// </summary>
    private void RotateMaterialChange()
    {
        // 子オブジェクトを取得
        gearChild = this.transform.GetChild(0).gameObject;
        // 回転可能回数がマテリアルの種類より少ない時
        if (gearRotateCount < SetGears.Instance.GetgearMaterialLength.Length)
        {
            // 子オブジェクトのマテリアルを変える
            Material[] mats = gearChild.GetComponent<MeshRenderer>().materials;
            mats[1] = SetGears.Instance.GetgearMaterial(gearRotateCount);
            gearChild.GetComponent<MeshRenderer>().materials = mats;
        }
    }
    // 各種ゲッター
    public sbyte GetgearNumber
    {
        get { return this.gearNumber; }
        private set { this.gearNumber = value; }
    }

}
