using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    public SceneHistoryManager sceneHistoryManager;  // SceneHistoryManagerへの参照

    public void OnBackButtonClicked()
    {
        sceneHistoryManager.GoBackToPreviousScene();  // 戻るボタンを押したら前のシーンに戻る
    }
}
