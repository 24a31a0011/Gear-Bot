using UnityEngine;
using UnityEngine.Events;

public class GimmcikGear : MonoBehaviour
{
    [SerializeField] UnityEvent activGimmcikGearEvent;
    [SerializeField] UnityEvent deactivateGimmickGearEvent;

    public void ActivGimmick()
    {
        activGimmcikGearEvent.Invoke();
    }

    public void DeactivateGimmick()
    {
        deactivateGimmickGearEvent.Invoke();
    }
}
