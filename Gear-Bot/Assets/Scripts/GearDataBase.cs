// 榊の担当スクリプト
// 生成したギアに情報を持たせるスクリプト
using UnityEngine;
using UnityEngine.UI;

public class GearDataBase : MonoBehaviour
{
    // どのボタンのギアかを格納する変数
    private sbyte gearNumber;
    // 生成したギアの回転可能回数を格納する変数
    private byte gearRotateCount;
    // ギアが回転した回数をカウントする変数
    private int gearRotating;
    // ギアの残り回転数を表示する変数
    // private Text gearText; (まだマージしていないプログラムに使うので提出用に一旦無効化します。)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ギアの子テキストを取得
        // gearText = this.gameObject.transform.GetChild(0).gameObject.GetComponent<Text>();
        // RotateCountChange();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// ギアに情報を受け渡す関数
    /// </summary>
    public void SetGearDateBase(sbyte number, byte count)
    {
        GetgearNumber = number;
        GetgearRotateCount = count;
        Debug.Log(this.gearNumber + " : " + this.gearRotateCount);
        // RotateCountChange();
    }
    /// <summary>
    /// そのギアの回転可能回数を減らす関数
    /// </summary>
    public void RotatingGear()
    {
        GetgearRotating += 1;
        GetgearRotateCount -= 1;
    }
    /// <summary>
    /// そのギアの回転した回数を返す変数
    /// </summary>
    /// <returns></returns>
    public int ReturnRotateCount()
    {
        return GetgearRotating;
    }
    private void RotateCountChange()
    {
        // gearText.text = GetgearRotateCount.ToString();
    }
    // 各種ゲッター
    public sbyte GetgearNumber
    {
        get { return this.gearNumber; }
        private set { this.gearNumber = value; }
    }

    public byte GetgearRotateCount
    {
        get { return this.gearRotateCount; }
        set { this.gearRotateCount = value; }
    }
    public int GetgearRotating
    {
        get { return this.gearRotating; }
        private set { this.gearRotating = value; }
    }

}
