using UnityEngine;
using UnityEngine.UI;

public class SetSlider : MonoBehaviour
{
    [SerializeField] Slider bgmSlider;
    [SerializeField] Slider seSlider;

    void Start()
    {
        Audio.Instance.SetSlider(bgmSlider, seSlider);
    }
}
