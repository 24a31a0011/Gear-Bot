using UnityEngine;

public class GearClickSet : MonoBehaviour
{
    private GameObject clickGear; // クリックしたギアを代入する変数

    private GameObject dropPointObj; // ギアを設置するポイントを代入する変数

    private bool setMode = true; // 設置モードかどうかのフラグ

    private bool iscansetGear = true; // ギアを設置できるかどうかのフラグ

    [SerializeField] private GearDefault geardefault; // ギアの情報が格納されているスクリプト
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 設置モードになっている時だけ機能するようにする
        if (setMode)
        {
            // ギアが代入されている時
            if (clickGear != null)
            {
                dropPointObj = null;

                iscansetGear = true;

                Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition); // レイを作成
                RaycastHit mouseHit = new RaycastHit();
                // カーソルから飛ばしたRayに当たったオブジェクトを追加する
                if (Physics.Raycast(mouseRay, out mouseHit) && mouseHit.collider.gameObject.GetComponent<DropPointDefault>())
                {
                    dropPointObj = mouseHit.collider.gameObject;

                    SearchObject();
                }

                if (Input.GetMouseButtonDown(0))
                {
                    // 設置ポイントをクリック & 設置ポイント以外に重なっているものが無い時
                    if (dropPointObj != null && iscansetGear)
                    {
                        // 設置ポイントにギアを設置する
                        clickGear.transform.position = dropPointObj.transform.position;
                        // 設置してるフラグをオンにする
                        clickGear.gameObject.GetComponent<GearDefault>().Getsetnowgear = true;
                    }
                    // クリックした時代入したギアを忘れる
                    clickGear = null;
                }
            }
            // ギアを代入されていない時
            else
            {
                // クリック時ギアをクリックしたかを調べる
                if (Input.GetMouseButtonDown(0))
                {
                    clickGear = null; // 変数を初期化する

                    Ray clickRay = Camera.main.ScreenPointToRay(Input.mousePosition); // レイを作成
                    RaycastHit clickHit = new RaycastHit();

                    // クリックから飛ばしたRayにギアが当たったら代入する
                    if (Physics.Raycast(clickRay, out clickHit) && clickHit.collider.gameObject.GetComponent<GearDefault>())
                    {
                        // ギアを代入する
                        clickGear = clickHit.collider.gameObject;
                        // 代入したギアから変数を取得する
                        bool isinstalled = clickGear.gameObject.GetComponent<GearDefault>().Getsetnowgear;
                        Vector3 defaultVector = clickGear.gameObject.GetComponent<GearDefault>().GetGearDefaultVec;
                        // 設置済みギアの時
                        if (isinstalled)
                        {
                            clickGear.transform.position = defaultVector;
                            clickGear.gameObject.GetComponent<GearDefault>().Getsetnowgear = false;
                            // 代入したギアを忘れる
                            clickGear = null;
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// setMode変数を取得する関数
    /// </summary>
    public bool GetsetMode
    {
        get { return this.setMode; }
        set { this.setMode = value; }
    }
    /// <summary>
    /// 設置ポイントの周りを探索する関数
    /// </summary>
    private void SearchObject()
    {
        // ギアのコライダーを取得する
        SphereCollider clickcol= clickGear.GetComponent<SphereCollider>();

        RaycastHit[] hits = Physics.SphereCastAll(
                                            dropPointObj.transform.position,
                                            clickcol.transform.localScale.z * 90,
                                            Vector3.forward);
        Debug.Log(clickcol.transform.localScale.z * 90);

        foreach (var hit in hits)
        {
            // 探知範囲に配置ポイント以外の何かが見つかった時
            if (!hit.collider.gameObject.GetComponent<DropPointDefault>())
            {
                iscansetGear = false;
                Debug.Log("見つかったよ");
            }
        }
    }
}
