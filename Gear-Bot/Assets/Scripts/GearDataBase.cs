// å‚Ì’S“–ƒXƒNƒŠƒvƒg
// ¶¬‚µ‚½ƒMƒA‚Éî•ñ‚ğ‚½‚¹‚éƒXƒNƒŠƒvƒg
using UnityEngine;

public class GearDataBase : MonoBehaviour
{
    // ‚Ç‚Ìƒ{ƒ^ƒ“‚ÌƒMƒA‚©‚ğŠi”[‚·‚é•Ï”
    private sbyte gearNumber;
    // ¶¬‚µ‚½ƒMƒA‚Ì‰ñ“]‰Â”\‰ñ”‚ğŠi”[‚·‚é•Ï”
    private byte gearRotateCount;
    // ƒMƒA‚ª‰ñ“]‚µ‚½‰ñ”‚ğƒJƒEƒ“ƒg‚·‚é•Ï”
    private int gearRotating;
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
    /// <summary>
    /// ‚»‚ÌƒMƒA‚Ì‰ñ“]‰Â”\‰ñ”‚ğŒ¸‚ç‚·ŠÖ”
    /// </summary>
    public void RotatingGear()
    {
        GetgearRotating += 1;
        GetgearRotateCount -= 1;
    }
    /// <summary>
    /// ‚»‚ÌƒMƒA‚Ì‰ñ“]‚µ‚½‰ñ”‚ğ•Ô‚·•Ï”
    /// </summary>
    /// <returns></returns>
    public int ReturnRotateCount()
    {
        return GetgearRotating;
    }
    // ŠeíƒQƒbƒ^[
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
