using UnityEngine;

public class SampleGimmick : MonoBehaviour
{
    private bool isMoving = false;

    void Update()
    {
        if (isMoving && transform.position.x <= 0)
        {
            transform.position += new Vector3(1f, 0, 0) * Time.deltaTime;
        }
    }

    public void OnGimmick()
    {
        isMoving = true;
    }
}
