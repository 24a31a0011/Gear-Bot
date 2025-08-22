using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UI; // UIを扱うために必要

public class ClearScreen : MonoBehaviour
{
    public GoalScript End; // GoalManagerへの参照
    public GameObject goalUI; // ゴール達成時に表示するUIのGameObject

    void Start()
    {
        // UI非表示にしておく
        goalUI.SetActive(false);
    }

    void Update()
    {
        // GoalManagerのgoalReachedがtrueになった時にUIを表示
        if (End != null && End.clear)
        {
            // UIを表示
            goalUI.SetActive(true);
            Debug.Log("clear");
        }
    }
}
