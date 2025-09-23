using UnityEngine;
using UnityEngine.UI;

public class SpeedButton : MonoBehaviour
{
    [SerializeField] private Sprite[] speedSprites;
    // ƒCƒ“ƒXƒyƒNƒ^[‚Å 1”{,2”{,3”{—p‚ÌSprite‚ğ‡”Ô‚É“o˜^
    private Button button;
    private int currentSpeedIndex = 0;
    private readonly float[] speedScales = { 1f, 2f, 3f };

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ChangeSpeed);

        // ‰Šú‰»
        ApplySpeed();
    }

    private void ChangeSpeed()
    {
        currentSpeedIndex++;
        if (currentSpeedIndex >= speedScales.Length)
            currentSpeedIndex = 0;

        ApplySpeed();
    }

    private void ApplySpeed()
    {
        // ”{‘¬Ø‚è‘Ö‚¦
        Time.timeScale = speedScales[currentSpeedIndex];

        // Button ‚Ì‰æ‘œ‚ğØ‚è‘Ö‚¦
        if (button.image != null && speedSprites.Length > currentSpeedIndex)
        {
            button.image.sprite = speedSprites[currentSpeedIndex];
        }
    }
}
