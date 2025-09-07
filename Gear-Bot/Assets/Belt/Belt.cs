using UnityEngine;

public class Belt : MonoBehaviour
{
    [Header("コンベアベルト制御")]
    public ConveyorBelt targetBelt;

    public void TurnOn()
    {
        if (targetBelt == null)
        {
            Debug.Log("エラー: targetBeltが設定されていません！");
            return;
        }

        targetBelt.TurnOn();
    }
}
