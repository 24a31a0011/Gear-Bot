using UnityEngine;

public class GoalScript : MonoBehaviour
{
    public bool coregoal;  // ゴールにボールが入ったか
    public bool playergoal; // ゴールに選手が入ったか

    public bool clear = false; // ゴール成立の判定
    private string playerName; // 入ったオブジェクトの名前（選手用）

    private string carryerTag = "Player";  // ラグビー選手（ボールを持った）のタグ

    [SerializeField] private GameObject player; // ラグビー選手（ボールを持っている）
    [SerializeField] private GameObject core;  // ボールオブジェクト

    void Start()
    {

    }

    void Update()
    {
        // 更新処理は特に必要ないので空のままでOK
    }

    private void OnTriggerEnter(Collider collider)
    {
        // ゴールに入ったオブジェクトの名前を取得
        string enteredObjectName = collider.gameObject.name;

        // ボールがゴールに入ったとき
        if (enteredObjectName == core.name)
        {
            coregoal = true;
            Debug.Log(core.name + "がゴール！" + coregoal);
        }

        // 選手がゴールに入ったとき
        if (collider.CompareTag(carryerTag))
        {
            playergoal = true;
            Debug.Log(collider.gameObject.name + "がゴール！" + playergoal);
        }

        // 両方の条件が満たされた場合
        if (coregoal && playergoal)
        {
            clear = true; // ゴール成立
            Debug.Log("ゴール成立！");
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        // ゴールから出た場合、フラグをリセット
        if (collider.CompareTag(carryerTag))
        {
            playergoal = false;
        }

        if (collider.gameObject.name == core.name)
        {
            coregoal = false;
        }

        // ゴールから出た際、clearフラグをリセット
        if (!coregoal || !playergoal)
        {
            clear = false;
        }
    }
}
