using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("Canvas‚ğİ’è")]
    [SerializeField] GameObject OpenObjectCanvas;
    [SerializeField] GameObject CloseOptionCanvas;

    [Header("Scene‚Ì–¼‘O")]
    [SerializeField] string StageSelectScene;

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
        OpenObjectCanvas.SetActive(true);
        CloseOptionCanvas.SetActive(false);
    }

    // Option‚ğ•Â‚¶‚é
    public void CloseOption()
    {
        OpenObjectCanvas.SetActive(false);
        CloseOptionCanvas.SetActive(true);
    }
}
