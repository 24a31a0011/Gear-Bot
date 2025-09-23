///
/// çÏê¨é“ : ÉOÉGÉì
///
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private bool isAnimating = false;

    public void SetTrueAnime()
    {
        isAnimating = true;
    }

    public void SetFalseAnime()
    {
        isAnimating = false;
    }

    public bool GetIsAnimating()
    {
        return isAnimating;
    }
}
