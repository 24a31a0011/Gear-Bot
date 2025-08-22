using UnityEngine;
using UnityEngine.EventSystems;

public class core : MonoBehaviour
{
    public void OnCollisionStay(Collision collision)
    {
        // 衝突したオブジェクトからConveyorコンポーネントを取得
        var conveyor = collision.gameObject.GetComponent<Conveyor>();
        GetComponent<Rigidbody>().sleepThreshold = -1;
        if (conveyor != null)
        {
            conveyor.OnCollisionStay2(GetComponent<Collider>());
        }           
    }
}
