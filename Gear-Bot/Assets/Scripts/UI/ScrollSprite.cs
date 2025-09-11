///
/// 作成者 : グエン
///
using System.Linq;
using UnityEngine;

public class ScrollSprite : MonoBehaviour
{
    [Header("表示する場所")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("表示する画像")]
    [SerializeField] private Sprite[] sprites;

    // 現在表示している画像番号
    private int currentSpriteIndex = 0;

    private void Start()
    {
        if (spriteRenderer == null || sprites[0] == null) return;

        spriteRenderer.sprite = sprites[currentSpriteIndex];
    }

    public void OnRButton()
    {
        if (sprites[currentSpriteIndex] == sprites.Last()) return;
        currentSpriteIndex++;
        spriteRenderer.sprite = sprites[currentSpriteIndex];
    }

    public void OnLButton()
    {
        if (sprites[currentSpriteIndex] == sprites[0]) return;
        currentSpriteIndex--;
        spriteRenderer.sprite = sprites[currentSpriteIndex];
    }
}
