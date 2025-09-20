using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    [Header("Canvasを設定")]
    [SerializeField] GameObject OpenObjectCanvas;
    [SerializeField] GameObject CloseOptionCanvas;

    [SerializeField] private Image blocker;         // 全画面を覆う透明なImage

    void Start()
    {
        OpenObjectCanvas.SetActive(false);
    }

    void OnEnable()
    {
        blocker.raycastTarget = true;  // 有効時はブロックON
    }

    void OnDisable()
    {
        blocker.raycastTarget = false; // 無効時はブロックOFF
    }

    // 現在のsceneを再読み込みする
    public void OnResetButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Optionを開く
    public void OpenOption()
    {
        OnEnable();
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Decision);

        OpenObjectCanvas.SetActive(true);
        CloseOptionCanvas.SetActive(false);
    }

    // Optionを閉じる
    public void CloseOption()
    {
        OnDisable();
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Cansel);

        OpenObjectCanvas.SetActive(false);
        CloseOptionCanvas.SetActive(true);
    }
}
