///
/// ì¬Ò : ƒOƒGƒ“
///
using UnityEngine;

public class Arm : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Activate()
    {
        int current = animator.GetInteger("Arm");
        animator.SetInteger("Arm", current + 1);
    }
}
