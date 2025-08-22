using UnityEngine;

public class CensorTrigger : MonoBehaviour
{
    private Rigidbody parentRb;

    private void Start()
    {
        // 親オブジェクトの Rigidbody を取得
        parentRb = transform.parent.GetComponent<Rigidbody>();
        if (parentRb == null)
        {
            Debug.LogWarning("親オブジェクトにRigidbodyがありません");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (parentRb != null)
        {
            // 回転制約を解除（Z軸の回転フリーズ解除）
            parentRb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;

            // X,Y,Z速度をゼロにする
            parentRb.linearVelocity = Vector3.zero;

            // 親オブジェクトをZ軸で90度回転させる
            parentRb.transform.Rotate(0f, 0f, 90f);

            Debug.Log("親のfreezeRotation解除");
        }
    }
}
