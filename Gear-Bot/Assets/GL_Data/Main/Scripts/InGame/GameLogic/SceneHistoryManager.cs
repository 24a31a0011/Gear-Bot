using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHistoryManager : MonoBehaviour
{
    // シーンを記録する
    public void RecordPreviousScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString("previousScene", currentScene);  // PlayerPrefsにシーン名を保存
        PlayerPrefs.Save();  // データを即座に保存
        Debug.Log("シーン名を保存しました: " + currentScene);
    }

    // 前のシーンに戻る
    public void GoBackToPreviousScene()
    {
        string previousScene = PlayerPrefs.GetString("previousScene", "");  // PlayerPrefsからシーン名を取得
        if (!string.IsNullOrEmpty(previousScene))
        {
            Debug.Log("前のシーンに戻ります: " + previousScene);
            SceneManager.LoadScene(previousScene);  // 前のシーンに戻る
        }
        else
        {
            Debug.LogWarning("前のシーンが保存されていません");
        }
    }
}
