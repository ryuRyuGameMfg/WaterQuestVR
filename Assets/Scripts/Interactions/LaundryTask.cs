using UnityEngine;

/// <summary>
/// 洗濯タスク（水を出す）
/// 洗濯オブジェクトの周りで水を出すとタスクとして記録
/// </summary>
public class LaundryTask : WaterReceiver
{
    [Header("Laundry Settings")]
    [SerializeField] private float qualityDecrease = 3f;     // 水質低下

    protected override void Awake()
    {
        base.Awake();

        // コップまたはバケツを受け付ける（null = すべて）
        allowedVesselType = null; // すべての器具を受け付ける
        // conditionType はインスペクターで設定（デフォルト: TiltDetection）
        oneTimeExecution = true;
    }

    protected override void ConsumeWater()
    {
        if (currentContainer == null) return;

        // 水を空にする
        currentContainer.EmptyWater();

        // ログ出力
        Debug.Log($"[{gameObject.name}] 洗濯をしました。消費水量: {waterConsumption:F0}L");

        // GameManagerに記録（洗濯タスクとして）
        // 新しいRecordLaundryメソッドを追加するか、既存のRecordFarmingを使用
        GameManager.Instance.RecordFarming(waterConsumption); // 暫定的にRecordFarmingを使用
        GameManager.Instance.Data.Stamina -= staminaCost;
    }
}

