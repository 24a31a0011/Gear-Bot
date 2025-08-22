using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectScene : MonoBehaviour
{
    public SceneHistoryManager sceneHistoryManager;  // SceneHistoryManagerへの参照

    public void LoadTitle()
    {
        sceneHistoryManager.RecordPreviousScene();  // 現在のシーンを記録
        SceneManager.LoadScene("Title_Screen");
    }

    public void LoadStart()
    {
        sceneHistoryManager.RecordPreviousScene();  // 現在のシーンを記録
        SceneManager.LoadScene("StageSelect");
    }

    public void LoadTutorial()
    {
        sceneHistoryManager.RecordPreviousScene();  // 現在のシーンを記録
        SceneManager.LoadScene("Tutorial");
    }

    public void LoadOption()
    {
        sceneHistoryManager.RecordPreviousScene();  // 現在のシーンを記録
        SceneManager.LoadScene("Option");
    }

    public void LoadQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void LoadStage()
    {
        sceneHistoryManager.RecordPreviousScene();  // 現在のシーンを記録
        SceneManager.LoadScene("αtest_Stage1");
    }
}
