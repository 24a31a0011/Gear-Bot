using UnityEngine;
using UnityEngine.SceneManagement;  // SceneManagerを使用するために必要

public class SceneController : MonoBehaviour
{
    // 現在のシーンをリスタートするメソッド
    public void RestartCurrentScene()
    {
        // 現在のシーン名を取得して再読み込み
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }

    // 次のステージへ遷移するメソッド
    public void LoadNextScene()
    {
        // 現在のシーンのインデックスを取得
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // 次のシーンをロード（次のシーンが存在する場合）
        if (currentSceneIndex + 1 < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(currentSceneIndex + 1);
        }
        else
        {
            Debug.Log("次のステージは存在しません");
        }
    }

    // メニュー画面に戻るメソッド（メニュー画面は最初のシーンとして設定されていると仮定）
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Title_Screen"); // メニュー画面のシーン名を指定
    }
}
