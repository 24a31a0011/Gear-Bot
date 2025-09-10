using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Audio : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static Audio Instance { get; private set; }

    //Audioミキサーを入れる
    [SerializeField] AudioMixer audioMixer;

    [Header("AudioSources")]
    [SerializeField] private AudioSource bgmSource;   // BGM 用
    [SerializeField] private AudioSource seSource;    // SE 用


    //それぞれのスライダーを入れる
    private Slider bgmSlider;
    private Slider seSlider;

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
        PlayBGM();
    }

    // ===== BGM 再生関連 =====
    public void PlayBGM(bool loop = true)
    {
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // ===== SE 再生関連 =====
    public void PlaySE()
    {
        seSource.PlayOneShot(seSource.clip);
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
}