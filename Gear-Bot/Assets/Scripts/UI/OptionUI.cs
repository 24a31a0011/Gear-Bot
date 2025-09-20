using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("Canvas‚ğİ’è")]
    [SerializeField] GameObject OpenObjectCanvas;
    [SerializeField] GameObject CloseOptionCanvas;

    void Start()
    {
        OpenObjectCanvas.SetActive(false);
    }

    void Update()
    {
        
    }

    // Œ»İ‚Ìscene‚ğÄ“Ç‚İ‚İ‚·‚é
    public void OnResetButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Option‚ğŠJ‚­
    public void OpenOption()
    {
        // SE‚ğÄ¶
        Audio.Instance.SetClip(Audio.SEClips.Decision);

        OpenObjectCanvas.SetActive(true);
        CloseOptionCanvas.SetActive(false);
    }

    // Option‚ğ•Â‚¶‚é
    public void CloseOption()
    {
        // SE‚ğÄ¶
        Audio.Instance.SetClip(Audio.SEClips.Cansel);

        OpenObjectCanvas.SetActive(false);
        CloseOptionCanvas.SetActive(true);
    }
}
