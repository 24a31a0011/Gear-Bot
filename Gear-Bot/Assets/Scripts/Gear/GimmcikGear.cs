///
/// çÏê¨é“ : ÉOÉGÉì
///
using UnityEngine;
using UnityEngine.Events;

public class GimmcikGear : MonoBehaviour
{
    [SerializeField] UnityEvent activGimmcikGearEvent;
    [SerializeField] UnityEvent deactivateGimmickGearEvent = null;

    public void ActivGimmick()
    {
        activGimmcikGearEvent.Invoke();
    }

    public void DeactivateGimmick()
    {
        if (deactivateGimmickGearEvent == null) return;
        deactivateGimmickGearEvent.Invoke();
    }
}
