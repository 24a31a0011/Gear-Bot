using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    // シングルトンパターン:どこからでもアクセス出来るように
    public static PlayerAnimation Instance { get; private set; }

    [SerializeField] Animator robotAnimator;

    private void Awake()
    {
        // シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void NextAnime()
    {
        robotAnimator.SetTrigger("Move");
    }
}
