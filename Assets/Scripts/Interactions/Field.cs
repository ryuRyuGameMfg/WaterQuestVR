using UnityEngine;

/// <summary>
/// 畑（水をまく）
/// </summary>
public class Field : WaterReceiver
{
    [Header("Field Settings")]
    [SerializeField] private float qualityDecrease = 50f;    // 水質低下

    protected override void Awake()
    {
        base.Awake();

        // バケツのみ受け付ける
        allowedVesselType = typeof(WaterBucket);
        // 傾ける条件を設定
        conditionType = ConditionType.TiltDetection;
        oneTimeExecution = true;
    }

    protected override void ConsumeWater()
    {
        if (currentContainer == null) return;

        // 既にWaterVessel.Update()でEmptyWater()が呼ばれている可能性がある
        // ここでは水量の記録のみを行う
        float amount = currentContainer.MaxCapacity;

        // まだ満タンの場合は空にする（念のため）
        if (currentContainer.IsFull)
        {
            currentContainer.EmptyWater();
        }

        // ログ出力
        Debug.Log($"[{gameObject.name}] 畑に水をまきました。消費水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}");

        // GameManagerに記録
        GameManager.Instance.RecordFarming(amount, qualityDecrease, staminaCost);
    }
}
