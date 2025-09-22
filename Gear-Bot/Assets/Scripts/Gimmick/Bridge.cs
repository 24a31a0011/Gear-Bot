///
/// çÏê¨é“ : ÉOÉGÉì
///
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
        Debug.Log("hi");
        animator.SetBool("Open", false);
    }

    public bool GetActiv()
    {
        return animator.GetBool("Open");
    }
}
