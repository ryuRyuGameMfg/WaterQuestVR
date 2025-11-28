using UnityEngine;

/// <summary>
/// 洗濯（水を注ぐとタスク完了）
/// WaterVesselから水を注がれた時に洗濯タスクを記録
/// </summary>
public class Laundry : WaterReceiver
{
    [Header("洗濯設定")]
    [SerializeField] private float qualityDecrease = 3f;     // 水質低下

    protected override void Awake()
    {
        base.Awake();

        // すべての器具を受け付ける
        allowedVesselTypeEnum = AllowedVesselType.All;
        oneTimeExecution = true;
    }

    /// <summary>
    /// 水を注がれた時のタスク実行
    /// </summary>
    protected override void ExecuteTask(float amount, float quality)
    {
        // ログ出力
        Debug.Log($"[{gameObject.name}] 洗濯をしました。消費水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}");

        // GameManagerに記録（洗濯タスクとして）
        GameManager.Instance.RecordLaundry(amount, qualityDecrease, staminaCost);
    }
}

