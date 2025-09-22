using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[System.Serializable]
public class TutorialPage
{
    public Sprite sprite;              // 表示する画像
    public VideoClip videoClip;        // 再生する動画 (なければ null)
    public RawImage videoTarget;       // 表示先 (インスペクターで指定)
}

public class ScrollSprite : MonoBehaviour
{
    [Header("表示する場所")]
    [SerializeField] private Image tutorialImage;

    [Header("ページごとのデータ")]
    [SerializeField] private List<TutorialPage> pages = new List<TutorialPage>();

    [Header("切替ボタン")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("動画再生コンポーネント")]
    [SerializeField] private VideoPlayer videoPlayer;

    private int currentPageIndex = 0;

    private void Start()
    {
        if (tutorialImage == null || pages.Count == 0) return;
        UpdatePage();
    }

    public void OnRButton()
    {
        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePage();
        }
    }

    public void OnLButton()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePage();
        }
    }

    private void UpdatePage()
    {
        var page = pages[currentPageIndex];

        // 画像の切替
        if (page.sprite != null)
        {
            tutorialImage.sprite = page.sprite;
        }

        // 動画の切替
        if (videoPlayer != null)
        {
            if (page.videoClip != null && page.videoTarget != null)
            {
                // 動画ターゲットを有効化
                page.videoTarget.gameObject.SetActive(true);

                // RawImage に出力する設定
                videoPlayer.targetTexture = page.videoTarget.texture as RenderTexture;
                videoPlayer.clip = page.videoClip;
                videoPlayer.Play();
            }
            else
            {
                // 動画が無い or ターゲットが無い場合は停止
                videoPlayer.Stop();

                // 他の RawImage を全部オフ
                foreach (var p in pages)
                {
                    if (p.videoTarget != null)
                        p.videoTarget.gameObject.SetActive(false);
                }
            }
        }

        // ボタンの有効/無効化
        if (leftButton != null) leftButton.interactable = (currentPageIndex > 0);
        if (rightButton != null) rightButton.interactable = (currentPageIndex < pages.Count - 1);
    }
}
