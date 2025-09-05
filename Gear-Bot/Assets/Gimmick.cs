using UnityEngine;

public class Gimmick : MonoBehaviour
{
    [Header("コンベアベルト制御")]
    public ConveyorBelt targetBelt;

    [Header("操作設定")]
    public KeyCode activationKey = KeyCode.Space;
    public bool startOn = true;

    [Header("デバッグ")]
    public bool showDebugLogs = true;

    private void Start()
    {
        if (!ValidateSetup())
        {
            enabled = false;
            return;
        }

        if (startOn)
        {
            ActivateBelt();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(activationKey))
        {
            ActivateBelt();
        }
    }

    /// <summary>
    /// コンベアベルトを1回ONにする（1マス移動）
    /// </summary>
    public void ActivateBelt()
    {
        if (targetBelt == null)
        {
            LogDebug("エラー: targetBeltが設定されていません！", true);
            return;
        }

        targetBelt.TurnOn();
        LogDebug("コンベア ON");
    }

    /// <summary>
    /// 設定の妥当性をチェック
    /// </summary>
    private bool ValidateSetup()
    {
        if (targetBelt == null)
        {
            LogDebug("エラー: Target Beltが設定されていません！Inspectorで設定してください。", true);
            return false;
        }
        return true;
    }

    /// <summary>
    /// デバッグログ出力
    /// </summary>
    private void LogDebug(string message, bool forceLog = false)
    {
        if (showDebugLogs || forceLog)
        {
            if (forceLog && message.Contains("エラー"))
                Debug.LogWarning($"[Gimmick] {message}");
            else
                Debug.Log($"[Gimmick] {message}");
        }
    }

    /// <summary>
    /// Inspector警告（Editor専用）
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (targetBelt == null && Application.isPlaying)
        {
            Debug.LogWarning($"[Gimmick] {gameObject.name}: Target Beltが設定されていません");
        }
    }
}
