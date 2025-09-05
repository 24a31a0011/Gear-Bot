///
/// 作成者 : グエン
///
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
    [SerializeField] private Material dragMaterial;

    private MeshRenderer meshRenderer;

    // 現在のState
    private GridState currentState = GridState.Normal;

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
        if (eventData.button == PointerEventData.InputButton.Right && SceneManager.GetActiveScene().name != "Stage1")
        {
            if ((currentState & GridState.OnClick) == 0 && SetGears.Instance.CheckGearPlacement())
            {
                // OnClick を追加
                currentState |= GridState.OnClick;
                meshRenderer.material = offMaterial;
                SetGears.Instance.SetGear(this.transform);
            }
            else if ((currentState & GridState.OnClick) != 0)
            {
                // OnClick を解除
                currentState &= ~GridState.OnClick;
                meshRenderer.material = (currentState & GridState.OnDrag) != 0 ? dragMaterial : onMaterial; ;

                GearDataBase gear = GetComponentInChildren<GearDataBase>();
                if (gear != null)
                {
                    SetGears.Instance.RemoveGear(gear.gameObject);
                }
            }
        }
    }

    // マウスポインタがオブジェクトと重なっている
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 左ドラッグ中なら即塗り
        if (GridManager.Instance.GetIsLeftDragging())
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
        if ((currentState & GridState.OnDrag) != 0)
        {
            GridManager.Instance.UnregisterRouteGrid(this);
            routeNumber = 0;
            currentState &= ~GridState.OnDrag;
            meshRenderer.material = offMaterial;
        }
    }

    /// <summary>
    /// 左クリック時の処理
    /// </summary>
    private void OnDrag()
    {
        if ((currentState & GridState.OnDrag) == 0)
        {
            if (!GridManager.Instance.CheckDistance(this.transform))
                return;

            currentState |= GridState.OnDrag; // OnDrag を追加
            meshRenderer.material = dragMaterial;
            GridManager.Instance.RegisterRouteGrid(this);
        }
        else if ((currentState & GridState.OnDrag) != 0)
        {
            if (!GridManager.Instance.CheckLastRoute(this))
                return;

            // OnDrag を解除
            currentState &= ~GridState.OnDrag;
            meshRenderer.material = offMaterial;
            GridManager.Instance.UnregisterRouteGrid(this);
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
