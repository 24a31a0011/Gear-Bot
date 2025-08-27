using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public void OnClickReturn()
    {
        Bit_SceneController.Instance.ReturnScene();
    }

    public void OnClickGameStart()
    {
        Bit_SceneController.Instance.TitleScene();
    }

    public void OnClickStageSelect()
    {
        Bit_SceneController.Instance.GameStart();
    }

    public void OnClickTutorial()
    {
        Bit_SceneController.Instance.TutorialStart();
    }

    public void OnClickOption()
    {
        Bit_SceneController.Instance.OptionStart();
    }

    public void OnClickEnd()
    {
        Bit_SceneController.Instance.EndGame();
    }

    public void OnClickStageChange(int stageNum)
    {
        Bit_SceneController.Instance.StageChange(stageNum);
    }
}
