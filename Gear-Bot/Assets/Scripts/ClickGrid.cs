///
/// 作成者 : グエン
///
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickGrid : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum GridState
    {
        Normal,
        OnClick,
        OnDrag
    }

    [Header("Gridの色")]
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material ClickMaterial;
    [SerializeField] private Material dragMaterial;

    [SerializeField] private SetGears setGears;

    private MeshRenderer meshRenderer;

    public GridState currentState = GridState.Normal;

    // ルートとして使われた際の番号を格納
    public int routeNumber = 0;

    private void Start()
    {
        // 自身をGearManagerに登録する
        if (GridManager.Instance != null)
        {
            GridManager.Instance.RegisterClickGrid(this);
        }
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = offMaterial;
    }

    // クリックされたらStateを変更する
    public void OnPointerClick(PointerEventData eventData)
    {
        // 左クリック時の処理
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 現在のStateによりクリックした時の処理を変更
            if (currentState == GridState.Normal)
            {
                currentState = GridState.OnClick;
                meshRenderer.material = ClickMaterial;

            }
            else if (currentState == GridState.OnClick)
            {
                currentState = GridState.Normal;
                meshRenderer.material = onMaterial;
            }
        }
    }

    // マウスポインタがオブジェクトと重なっている
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 右ドラッグ中なら即塗り
        if (GridManager.Instance.GetIsRightDragging())
        {
            OnDrag();
        }
        else if (currentState == GridState.Normal)
        {
            meshRenderer.material = onMaterial;
        }
    }

    // マウスポインタがオブジェクトと重なっていない
    public void OnPointerExit(PointerEventData eventData)
    {
        if (currentState == GridState.Normal)
        {
            meshRenderer.material = offMaterial;
        }
    }

    /// <summary>
    /// 現在のStateをNormalにする
    /// </summary>
    public void ResetState()
    {
        if (currentState == GridState.OnDrag)
        {
            GridManager.Instance.UnregisterRouteGrid(this);
            routeNumber = 0;
            currentState = GridState.Normal;
            meshRenderer.material = offMaterial;
        }
    }

    /// <summary>
    /// 右クリック時の処理
    /// </summary>
    private void OnDrag()
    {
        if (currentState != GridState.OnDrag)
        {
            if (!GridManager.Instance.CheckDistance(this.transform))
                return;

            currentState = GridState.OnDrag;
            meshRenderer.material = dragMaterial;
            GridManager.Instance.RegisterRouteGrid(this);
        }
        else if (currentState == GridState.OnDrag)
        {
            if (!GridManager.Instance.CheckLastRoute(this))
                return;

            ResetState();
        }
    }

    public void SetRouteNumber(int num)
    {
        routeNumber = num;
    }

    public void OnDragByManager()
    {
        if (currentState != GridState.OnDrag)
        {
            currentState = GridState.OnDrag;
            meshRenderer.material = dragMaterial;
            GridManager.Instance.RegisterRouteGrid(this);
        }
        else if (currentState == GridState.OnDrag)
        {
            if (!GridManager.Instance.CheckLastRoute(this))
                return;

            ResetState();
        }
    }
}
