using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public void OnClickReturn()
    {
        SceneController.Instance.ReturnScene();
    }

    public void OnClickGameStart()
    {
        SceneController.Instance.TitleScene();
    }

    public void OnClickStageSelect()
    {
        SceneController.Instance.GameStart();
    }

    public void OnClickTutorial()
    {
        SceneController.Instance.TutorialStart();
    }

    public void OnClickOption()
    {
        SceneController.Instance.OptionStart();
    }

    public void OnClickEnd()
    {
        SceneController.Instance.EndGame();
    }

    public void OnClickStageChange(int stageNum)
    {
        SceneController.Instance.StageChange(stageNum);
    }
}
