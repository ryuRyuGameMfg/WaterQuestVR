using UnityEngine;

/// <summary>
/// 畑（水をまく）
/// </summary>
public class Field : WaterReceiver
{
    [Header("Field Settings")]
    [SerializeField] private float qualityDecrease = 5f;     // 水質低下

    protected override void Awake()
    {
        base.Awake();

        // バケツのみ受け付ける
        allowedVesselType = typeof(WaterBucket);
        // conditionType はインスペクターで設定（デフォルト: TiltDetection）
        oneTimeExecution = true;
    }

    protected override void ConsumeWater()
    {
        if (currentContainer == null) return;

        // 水を空にする
        currentContainer.EmptyWater();

        // ログ出力
        Debug.Log($"[{gameObject.name}] 畑に水をまきました。消費水量: {waterConsumption:F0}L");

        // GameManagerに記録
        GameManager.Instance.RecordFarming(waterConsumption);
        GameManager.Instance.Data.Stamina -= staminaCost;
    }
}
