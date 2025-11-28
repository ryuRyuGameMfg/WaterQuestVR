using UnityEngine;

/// <summary>
/// 飲用ポイント（口元で水を飲むとタスク完了）
/// WaterVesselから水を注がれた時に飲用タスクを記録
/// </summary>
public class DrinkingPoint : WaterReceiver
{
    [Header("飲用設定")]
    [SerializeField] private float safeStaminaGain = 10f;         // 安全な水の体力回復
    [SerializeField] private float unsafeStaminaLoss = 10f;       // 汚染水の体力減少
    [SerializeField] private float safeQualityThreshold = 80f;    // 安全閾値

    protected override void Awake()
    {
        base.Awake();

        // コップのみ受け付ける
        allowedVesselTypeEnum = AllowedVesselType.WaterCup;
        oneTimeExecution = true;
    }

    /// <summary>
    /// 水を注がれた時のタスク実行
    /// </summary>
    protected override void ExecuteTask(float amount, float quality)
    {
        bool isSafe = quality >= safeQualityThreshold;

        // ログ出力
        if (isSafe)
        {
            Debug.Log($"[{gameObject.name}] 安全な水を飲みました。水質: {quality:F0}、体力回復: +{safeStaminaGain}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 汚染された水を飲んでしまいました。水質: {quality:F0}、体力減少: -{unsafeStaminaLoss}");
        }

        // GameManagerに記録（体力変化も含む）
        GameManager.Instance.RecordDrinking(amount, quality, safeStaminaGain, unsafeStaminaLoss);
    }
}
