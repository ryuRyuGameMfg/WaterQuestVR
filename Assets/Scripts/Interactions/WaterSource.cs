using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 水を出すもの（蛇口など）
/// Obiエフェクトの表示/非表示で水の出し入れを制御
/// </summary>
public class WaterSource : WaterInteractionBase
{
    [Header("Water Effect")]
    [FormerlySerializedAs("obiWaterEffect")]
    [SerializeField] private GameObject obiWaterObject;      // Obiの水オブジェクト

    [Header("Water Settings")]
    [SerializeField] private float waterQuality = 100f;      // 汲める水の水質
    [SerializeField] private float staminaCost = 10f;        // 体力コスト
    [SerializeField] private float flowDuration = 2f;        // 水が出る時間（秒）

    private bool isWaterFlowing = false;

    protected override void Awake()
    {
        base.Awake();

        // デフォルト設定（インスペクターで変更可能）
        requiresFull = false; // 空の器具が必要
        // conditionType はインスペクターで設定（デフォルト: ButtonPress）
    }

    protected override void OnContainerEntered(WaterVessel container)
    {
        // CollisionDetectionの場合はOnTriggerEnterで既に実行済み
        // ここでは他の処理があれば実装
    }

    protected override void OnContainerExited(WaterVessel container)
    {
        // 範囲外に出たら水を止める
        StopWaterFlow();
    }

    protected override void ExecuteTask()
    {
        Debug.Log($"[{gameObject.name}] ExecuteTask()が呼ばれました。currentContainer={currentContainer?.name}, IsFull={currentContainer?.IsFull}, isWaterFlowing={isWaterFlowing}");

        if (currentContainer == null)
        {
            Debug.LogWarning($"[{gameObject.name}] currentContainerがnullです");
            return;
        }

        if (currentContainer.IsFull)
        {
            Debug.Log($"[{gameObject.name}] 器具は既に満タンです");
            return;
        }

        if (isWaterFlowing)
        {
            Debug.Log($"[{gameObject.name}] 既に水が流れています");
            return;
        }

        Debug.Log($"[{gameObject.name}] StartWaterFlow()を実行します");
        StartWaterFlow();
    }

    private void StartWaterFlow()
    {
        isWaterFlowing = true;
        isExecuting = true;

        // Obi水オブジェクトを有効化（水が出る）
        if (obiWaterObject != null)
        {
            obiWaterObject.SetActive(true);
        }

        // 器具に水を満タンにする
        currentContainer.FillWater(waterQuality);

        // ログ出力
        Debug.Log($"[{gameObject.name}] 水が注がれました。水量: {currentContainer.MaxCapacity:F0}L");

        // GameManagerに記録
        float amount = currentContainer.MaxCapacity;
        GameManager.Instance.RecordDrawWater(amount, waterQuality);
        GameManager.Instance.Data.Stamina -= staminaCost;

        // 一定時間後に水を止める
        Invoke(nameof(StopWaterFlow), flowDuration);
    }

    private void StopWaterFlow()
    {
        // Obi水オブジェクトを無効化（水が止まる）
        if (obiWaterObject != null)
        {
            obiWaterObject.SetActive(false);
        }

        Debug.Log($"[{gameObject.name}] 水が止まりました。");

        isWaterFlowing = false;
        isExecuting = false;
    }
}

