using UnityEngine;

/// <summary>
/// 畑（水をまくとタスク完了）
/// WaterVesselから水を注がれた時に農業タスクを記録
/// </summary>
public class Field : WaterReceiver
{
    [Header("畑設定")]
    [SerializeField] private float qualityDecrease = 50f;    // 水質低下

    protected override void Awake()
    {
        base.Awake();

        // バケツのみ受け付ける
        allowedVesselTypeEnum = AllowedVesselType.WaterBucket;
        oneTimeExecution = true;
    }

    /// <summary>
    /// 水を注がれた時のタスク実行
    /// </summary>
    protected override void ExecuteTask(float amount, float quality)
    {
        // ログ出力
        Debug.Log($"[{gameObject.name}] 畑に水をまきました。消費水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}");

        // GameManagerに記録
        GameManager.Instance.RecordFarming(amount, qualityDecrease, staminaCost);
    }
}
