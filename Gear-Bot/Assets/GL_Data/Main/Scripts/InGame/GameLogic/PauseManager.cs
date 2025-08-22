using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;  // ポーズメニューのUI（インスペクターで設定）
    private bool isPaused = false;  // ゲームがポーズ中かどうか

    private void Awake()
    {
        pauseMenu.SetActive(false);
    }
    void Update()
    {
        
    }

    // ポーズとアンポーズの切り替え
    void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // ゲームをポーズ状態に
            Time.timeScale = 0f;  // ゲーム内の時間を止める
            pauseMenu.SetActive(true);  // ポーズメニューを表示
        }
        else
        {
            // ゲームを再開
            Time.timeScale = 1f;  // ゲーム内の時間を元に戻す
            pauseMenu.SetActive(false);  // ポーズメニューを非表示
        }
    }

    public void Resume()
    {
        TogglePause();  // ポーズを解除
    }
}
