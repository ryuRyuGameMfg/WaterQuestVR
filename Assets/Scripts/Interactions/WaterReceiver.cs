using UnityEngine;

/// <summary>
/// 水を受けるもの（畑、飲用ポイントなど）
/// 器具から水を空にする処理
/// </summary>
public abstract class WaterReceiver : WaterInteractionBase
{
    [Header("Water Consumption Settings")]
    [SerializeField] protected float waterConsumption = 50f;   // 消費する水量
    [SerializeField] protected float staminaCost = 15f;        // 体力コスト

    [Header("One-Time Execution")]
    [Tooltip("1回のみ実行するか")]
    [SerializeField] protected bool oneTimeExecution = true;

    protected bool hasExecuted = false;

    protected override void Awake()
    {
        base.Awake();

        // デフォルト設定（インスペクターで変更可能）
        requiresFull = true; // 満タンの器具が必要
        // conditionType はインスペクターで設定（デフォルト: TiltDetection）
    }

    protected override void OnContainerEntered(WaterVessel container)
    {
        // 1回実行制限のリセット
        if (oneTimeExecution)
        {
            hasExecuted = false;
        }
    }

    protected override void OnContainerExited(WaterVessel container)
    {
        // 範囲外に出たら実行フラグをリセット
        hasExecuted = false;
    }

    protected override bool CheckCondition()
    {
        // 1回実行制限のチェック
        if (oneTimeExecution && hasExecuted) return false;

        return base.CheckCondition();
    }

    protected override void ExecuteTask()
    {
        if (currentContainer == null || !currentContainer.IsFull) return;
        if (oneTimeExecution && hasExecuted) return;

        ConsumeWater();
        hasExecuted = true;
        isExecuting = true;
    }

    /// <summary>
    /// 水を消費する処理（サブクラスで実装）
    /// </summary>
    protected abstract void ConsumeWater();
}

