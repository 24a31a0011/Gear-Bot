using UnityEngine;

public class SampleBridge : MonoBehaviour, IGimmick
{
    private bool isMoving = false;

    private float currentRotation = 0f; // ó›êœÇµÇΩâÒì]äp
    float speed = 45f; // 90ìx/ïb


    private void Update()
    {
        if (isMoving)
        {
            if (currentRotation < 90f)
            {
                // Ç±ÇÃÉtÉåÅ[ÉÄÇ≈âÒì]Ç∑ÇÈäpìx
                float deltaRotation = speed * Time.deltaTime;

                // écÇËÇÃäpìxÇí¥Ç¶Ç»Ç¢ÇÊÇ§Ç…í≤êÆ
                if (currentRotation + deltaRotation > 90f)
                {
                    deltaRotation = 90f - currentRotation;
                }

                transform.Rotate(0f, 0f, deltaRotation);
                currentRotation += deltaRotation;
            }
        }
        else
        {
            if (currentRotation > 0f)
            {
                float deltaRotation = speed * Time.deltaTime;
                if (currentRotation - deltaRotation < 0f)
                {
                    deltaRotation = currentRotation; // écÇËï™ÇæÇØñﬂÇ∑
                }

                transform.Rotate(0f, 0f, -deltaRotation);
                currentRotation -= deltaRotation;
            }
        }
    }

    public void Activate()
    {
        isMoving = true;
    }

    [ContextMenu("StopGimmick")]
    public void Deactivate()
    {
        isMoving = false;
    }
}
