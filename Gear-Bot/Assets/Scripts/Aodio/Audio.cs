///
/// 作成者 : グエン
///
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

public class Audio : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static Audio Instance { get; private set; }

    public enum SEClips
    {
        Cansel,
        Decision,
        StageClear,
        StageReset,
        Gear,
        ResetRoute,
        RouteStart
    }

    public enum BGMClips
    {
        DefaultBGM,
        StageBGM
    }

    //Audioミキサーを入れる
    [SerializeField] AudioMixer audioMixer;

    [Header("AudioSources")]
    [SerializeField] private AudioSource bgmSource;   // BGM 用
    [SerializeField] private AudioSource seSource;    // SE 用

    // SE
    [Header("SEClip")]
    [SerializeField] private AudioClip[] seClips;

    // BGM
    [Header("BGMClip")]
    [SerializeField] private AudioClip[] bgmClips;

    //それぞれのスライダーを入れる
    private Slider bgmSlider;
    private Slider seSlider;

    private AudioClip currentClip;

    private AudioClip currentBGMClip;

    private void Awake()
    {
        // すでにインスタンスが存在しているか確認
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 重複を防ぐ
            return;
        }

        // 自分をインスタンスに登録
        Instance = this;

        // シーンをまたいでも破棄されないように設定
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

    }

    // ===== BGM 再生関連 =====
    public void PlayBGM(bool loop = true)
    {
        if (currentBGMClip == null) return;
        if (bgmSource.clip == currentBGMClip) return;

        bgmSource.clip = currentBGMClip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // ===== SE 再生関連 =====
    private void PlaySE()
    {
        if (currentClip == null) return;

        seSource.PlayOneShot(currentClip);
    }

    // ===== 音量調整 =====
    public void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGM", value);
    }

    public void SetSEVolume(float value)
    {
        audioMixer.SetFloat("SE", value);
    }

    // ===== スライダー登録 =====
    public void SetSlider(Slider bgmSl, Slider seSl)
    {
        bgmSlider = bgmSl;
        seSlider = seSl;

        // 初期値を反映
        if (audioMixer.GetFloat("BGM", out float bgmVolume))
            bgmSlider.value = bgmVolume;

        if (audioMixer.GetFloat("SE", out float seVolume))
            seSlider.value = seVolume;

        // 値変更イベントを登録
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        seSlider.onValueChanged.AddListener(SetSEVolume);
    }

    public void SetClip(SEClips sEClips)
    {
        // enumごとに再生するSEを変える
        switch (sEClips)
        {
            case SEClips.Cansel:
                currentClip = seClips[0];
                break;
            case SEClips.Decision:
                currentClip = seClips[1];
                break;
            case SEClips.StageClear:
                currentClip = seClips[2];
                break;
            case SEClips.StageReset:
                currentClip = seClips[3];
                break;
            case SEClips.Gear:
                currentClip = seClips[4];
                break;
            case SEClips.ResetRoute:
                currentClip = seClips[5];
                break;
            case SEClips.RouteStart:
                currentClip = seClips[6];
                break;
        }

        PlaySE();
    }

    public void SetBGMClip(BGMClips bGMClips)
    {
        // enumごとに再生するBGMを変える
        switch (bGMClips)
        {
            case BGMClips.DefaultBGM:
                currentBGMClip = bgmClips[0];
                break;
            case BGMClips.StageBGM:
                currentBGMClip = bgmClips[1];
                break;
        }

        PlayBGM();
    }
}