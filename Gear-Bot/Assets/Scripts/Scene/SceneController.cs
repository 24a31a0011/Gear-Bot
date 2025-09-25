///
/// 作成者 : グエン
///
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class SceneController : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static SceneController Instance { get; private set; }


    [SerializeField] private string startScene;
    [SerializeField] private string tutorialScene;
    [SerializeField] private string optionScene;
    [SerializeField] private string titleScene;
    [SerializeField] private string clearScene;

    [SerializeField] List<string> mainGameStageList = new List<string>();

    // 一個前のscene名を記憶
    private string previousSceneName;
    // 現在のscene名を記憶
    private string currentSceneName;

    private FadeController fadeController;

    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            fadeController = GetComponent<FadeController>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ゲームを開始。ステージ選択画面に移行する
    public void GameStart()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Decision);

        previousSceneName = currentSceneName;
        LoadScene(startScene);
    }

    // チュートリアルへと移行
    public void TutorialStart()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Decision);

        previousSceneName = currentSceneName;
        LoadScene(tutorialScene);
    }

    // オプション画面へと移行
    public void OptionStart()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Decision);

        previousSceneName = currentSceneName;
        LoadScene(optionScene);
    }

    // sceneが読み込まれたら今いるsceneを記憶する
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        Debug.Log("currentSceneName:" + currentSceneName + "\npreviousSceneName:" + previousSceneName);

        if (mainGameStageList.Contains(scene.name))
        {
            // ステージ中
            Audio.Instance.SetBGMClip(Audio.BGMClips.StageBGM);
        }
        else
        {
            // それ以外
            Audio.Instance.SetBGMClip(Audio.BGMClips.DefaultBGM);
        }
    }

    // ゲームを終了する
    public void EndGame()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Decision);

        // エディター上で実行中かどうか
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // エディターを停止
#else
        Application.Quit(); // ビルド後のゲームを終了
#endif
    }

    // 一つ前のSceneへと戻る
    public void ReturnScene()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Cansel);

        LoadScene(previousSceneName);
        previousSceneName = currentSceneName;
    }

    // 引数で設定した番号のステージに移行

    /// <param name="stageNum">移行するステージの番号</param>

    public void StageChange(int stageNum)

    {

        previousSceneName = currentSceneName;

        if (stageNum < 0 || stageNum >= mainGameStageList.Count)

        {

            Debug.LogWarning($"無効なステージ番号: {stageNum}");

            return;

        }



        if (string.IsNullOrEmpty(mainGameStageList[stageNum]))

        {

            Debug.LogWarning($"ステージ名が設定されていません: {stageNum}");

            return;

        }

        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Decision);

        LoadScene(mainGameStageList[stageNum]);

    }


    public void ClearScene()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.StageClear);

        previousSceneName = currentSceneName;
        LoadScene(clearScene);
    }

    public void TitleScene()
    {
        // SEを再生
        Audio.Instance.SetClip(Audio.SEClips.Cansel);

        LoadScene(titleScene);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneSequence(sceneName));
    }

    private IEnumerator LoadSceneSequence(string sceneName)
    {
        // フェードアウト
        yield return StartCoroutine(fadeController.FadeOut());

        // シーン読み込み
        yield return SceneManager.LoadSceneAsync(sceneName);

        // フェードイン
        yield return StartCoroutine(fadeController.FadeIn());
    }

    public void NextScene()
    {
        // 現在のシーン名が mainGameStageList に含まれているか確認
        int currentIndex = mainGameStageList.IndexOf(previousSceneName);

        if (currentIndex == -1)
        {
            // 現在のシーンがリストにない場合、警告を表示して終了
            Debug.LogWarning($"現在のシーン '{previousSceneName}' が mainGameStageList に存在しません");
            return;
        }

        // 次のステージ（インデックス + 1）に進む
        int nextIndex = currentIndex + 1;

        if (nextIndex >= mainGameStageList.Count)
        {
            // 次のステージがリストの範囲外の場合、警告を表示して終了
            Debug.LogWarning("次のステージはありません");
            return;
        }

        string nextScene = mainGameStageList[nextIndex];

        // 次のステージに進む
        Audio.Instance.SetClip(Audio.SEClips.Decision); // SEの再生
        LoadScene(nextScene); // 次のステージを読み込む
    }

}
