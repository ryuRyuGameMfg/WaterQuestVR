using UnityEngine;

/// <summary>
/// 飲用ポイント（口元）
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
        allowedVesselType = typeof(WaterCup);
        // 傾ける条件を設定
        conditionType = ConditionType.TiltDetection;
        oneTimeExecution = true;
    }

    protected override void ConsumeWater()
    {
        if (currentContainer == null) return;

        // 既にWaterVessel.Update()でEmptyWater()が呼ばれている可能性がある
        // 水質は空になる前に取得する必要がある
        float quality = currentContainer.WaterQuality;
        float amount = currentContainer.MaxCapacity;
        bool isSafe = quality >= safeQualityThreshold;

        // まだ満タンの場合は空にする（念のため）
        if (currentContainer.IsFull)
        {
            currentContainer.EmptyWater();
        }

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
