using UnityEngine;

/// <summary>
/// 飲用ポイント（口元）
/// </summary>
public class DrinkingPoint : WaterReceiver
{
    [Header("Drinking Settings")]
    [SerializeField] private float safeStaminaGain = 10f;         // 安全な水の体力回復
    [SerializeField] private float unsafeStaminaLoss = 10f;       // 汚染水の体力減少
    [SerializeField] private float safeQualityThreshold = 80f;    // 安全閾値

    protected override void Awake()
    {
        base.Awake();

        // コップのみ受け付ける
        allowedVesselType = typeof(WaterCup);
        // conditionType はインスペクターで設定（デフォルト: TiltDetection）
        oneTimeExecution = true;
    }

    protected override void ConsumeWater()
    {
        if (currentContainer == null) return;

        // 水質チェック
        float quality = currentContainer.WaterQuality;
        bool isSafe = quality >= safeQualityThreshold;

        // 水を空にする
        currentContainer.EmptyWater();

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
        GameManager.Instance.RecordDrinking(waterConsumption, quality);
    }
}
