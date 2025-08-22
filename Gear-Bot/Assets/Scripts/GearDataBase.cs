// å‚Ì’S“–ƒXƒNƒŠƒvƒg
// ¶¬‚µ‚½ƒMƒA‚Éî•ñ‚ğ‚½‚¹‚éƒXƒNƒŠƒvƒg
using UnityEngine;

public class GearDataBase : MonoBehaviour
{
    // ‚Ç‚Ìƒ{ƒ^ƒ“‚ÌƒMƒA‚©‚ğŠi”[‚·‚é•Ï”
    private sbyte gearNumber;
    // ¶¬‚µ‚½ƒMƒA‚Ì‰ñ“]‰Â”\‰ñ”‚ğŠi”[‚·‚é•Ï”
    private byte gearRotateCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    /// <summary>
    /// ƒMƒA‚Éî•ñ‚ğó‚¯“n‚·ŠÖ”
    /// </summary>
    public void SetGearDateBase(sbyte number, byte count)
    {
        GetgearNumber = number;
        GetgearRotateCount = count;
        Debug.Log(this.gearNumber + " : " + this.gearRotateCount);
    }

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
}
