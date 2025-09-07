using UnityEngine;

public class Gimmick : MonoBehaviour
{
    [Header("コンベアベルト制御")]
    public ConveyorBelt targetBelt;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (targetBelt == null)
            {
                Debug.Log("エラー: targetBeltが設定されていません！");
                return;
            }

            targetBelt.TurnOn();
            Debug.Log("コンベア ON");
        }
    }
}
