///
/// 作成者 : グエン
///
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Arm : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("対象関連のデータ")]
    [SerializeField] private GameObject bag;
    [SerializeField] private Vector3 targetPos;

    [Header("アームの先端部分")]
    [SerializeField] private GameObject armTip;

    public void Activate()
    {
        int current = animator.GetInteger("Arm");

        animator.SetInteger("Arm", current + 1);
    }

    public void Get()
    {
        bag.transform.SetParent(armTip.transform);

        bag.transform.localPosition = Vector3.zero;
    }

    public void Unload()
    {
        bag.transform.SetParent(null);

        bag.transform.position = targetPos;
    }
}
