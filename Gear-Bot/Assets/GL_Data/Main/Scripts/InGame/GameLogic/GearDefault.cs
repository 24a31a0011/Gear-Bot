using UnityEngine;

public class GearDefault : MonoBehaviour
{
    // ギアがホットバーに居る時の座標
    private Vector3 gearDefaultVec;

    private bool setnowgear = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetGearVector();
    }

    // Update is called once per frame
    void Update()
    {
    }
    /// <summary>
    /// ギアがインベントリ内にある時の座標の値を返す
    /// </summary>
    public Vector3 GetGearDefaultVec
    {
        get { return gearDefaultVec; }
        set { this.gearDefaultVec = value; }
    }
    /// <summary>
    /// ギアが設置ポイントに付いているかのフラグを返す
    /// </summary>
    public bool Getsetnowgear
    {
        get { return setnowgear; }
        set { this.setnowgear = value; }
    }
    // ギアがインベントリ内にある時の座標を記憶する
    private void SetGearVector()
    {
        // transformを取得
        Transform gearDefaultTransform = this.transform;
        // インベントリ内での座標を記憶する
        gearDefaultVec = gearDefaultTransform.position;
    }
}