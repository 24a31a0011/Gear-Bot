using UnityEngine;
using UnityEngine.EventSystems;

public class Conveyor : MonoBehaviour
{
    [SerializeField]
    private float speed = 8f;

    // 0 = 停止, 1 = 回転, 2 = 逆回転
    [SerializeField]
    private int state = 0;

    [Header("Movement Direction")]
    [SerializeField]
    private Vector3 moveDirection = Vector3.right; // 移動方向

    public void OnCollisionStay2(Collider other)
    {
        if (!other.CompareTag("Core"))
            return;

        if (state == 0)
            return;

        Rigidbody rb = other.attachedRigidbody;

        if (rb != null)
        {
            Vector3 forceDirection = transform.TransformDirection(moveDirection);

            if (state == 1)
            {
                rb.linearVelocity = forceDirection;
            }
            else if (state == 2)
            {
                rb.linearVelocity = -forceDirection;
            }
        }
    }

    public void SetState(int newState)
    {
        if (newState >= 0 && newState <= 2)
        {
            state = newState;
        }
    }

    public void OnGimmick()
    {
        SetState(1);
    }
}

