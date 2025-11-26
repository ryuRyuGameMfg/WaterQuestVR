using UnityEngine;

/// <summary>
/// 洗濯（水を出す）
/// 洗濯オブジェクトの周りで水を出すとタスクとして記録
/// </summary>
public class Laundry : WaterReceiver
{
    [Header("Laundry Settings")]
    [SerializeField] private float qualityDecrease = 3f;     // 水質低下

    protected override void Awake()
    {
        base.Awake();

        // コップまたはバケツを受け付ける（null = すべて）
        allowedVesselType = null; // すべての器具を受け付ける
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
        Debug.Log($"[{gameObject.name}] 洗濯をしました。消費水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}");

        // GameManagerに記録（洗濯タスクとして）
        GameManager.Instance.RecordLaundry(amount, qualityDecrease, staminaCost);
    }
}

