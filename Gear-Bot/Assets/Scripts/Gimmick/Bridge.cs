using UnityEngine;

public class Bridge : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Activate()
    {
        animator.SetBool("Open", true);
    }

    public void Deactivate()
    {
        animator.SetBool("Open", false);
    }
}
