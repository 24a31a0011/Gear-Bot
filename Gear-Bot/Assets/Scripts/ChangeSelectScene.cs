///
/// çÏê¨é“ : ÉOÉGÉì
///
using UnityEngine;
using UnityEngine.UI;

public class ChangeSelectScene : MonoBehaviour
{
    [SerializeField] private GameObject StageCanvas1;
    [SerializeField] private GameObject StageCanvas2;

    [SerializeField] private float value = 1.5f;

    private void Start()
    {
        StageCanvas1.SetActive(true);
        StageCanvas2.SetActive(false);
    }

    public void OnClickRightButton()
    {
        StageCanvas1.SetActive(false);
        StageCanvas2.SetActive(true);
    }

    public void OnClickLeftButton()
    {
        StageCanvas1.SetActive(true);
        StageCanvas2.SetActive(false);
    }

    public void OnClickExpansionImage(GameObject islandObj)
    {
        islandObj.GetComponent<RectTransform>().localScale *= value;
    }

    public void OnClickReductionImage(GameObject islandObj)
    {
        islandObj.GetComponent<RectTransform>().localScale /= value;
    }
}
