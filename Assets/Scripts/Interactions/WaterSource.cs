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
    [Tooltip("汲める水の水質（0-100）")]
    [Range(0f, 100f)]
    [SerializeField] private float waterQuality = 100f;

    [Tooltip("水を汲むときの体力消費量")]
    [Range(0f, 100f)]
    [SerializeField] private float staminaCost = 10f;

    [Tooltip("水が出続ける時間（秒）\n• 2秒: 短時間\n• 5秒: 通常\n• 999999: 実質無限")]
    [Min(0f)]
    [SerializeField] private float flowDuration = 999999f;

    [Tooltip("器具が範囲外に出たときに自動的に水を止めるか")]
    [SerializeField] private bool stopOnExit = true;

    private bool isWaterFlowing = false;

    protected override void Awake()
    {
        Debug.Log($"[{gameObject.name}] WaterSource.Awake() 開始");
        base.Awake();

        // デフォルト設定（インスペクターで変更可能）
        requiresFull = false; // 空の器具が必要
        // conditionType はインスペクターで設定（デフォルト: ButtonPress）
        Debug.Log($"[{gameObject.name}] WaterSource.Awake() 完了。conditionType={conditionType}, requiresFull={requiresFull}");
    }

    protected override void Start()
    {
        base.Start();
        Debug.Log($"[{gameObject.name}] WaterSource.Start() 完了");
    }

    protected override void OnContainerEntered(WaterVessel container)
    {
        // CollisionDetectionの場合はOnTriggerEnterで既に実行済み
        // ここでは他の処理があれば実装
    }

    protected override void OnContainerExited(WaterVessel container)
    {
        // 範囲外に出たら水を止める（設定で制御可能）
        if (stopOnExit)
        {
            StopWaterFlow();
        }
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
        Debug.Log($"[{gameObject.name}] 水が注がれました。水量: {currentContainer.MaxCapacity:F0}L、水質: {waterQuality:F0}、体力消費: {staminaCost:F0}");

        // GameManagerに記録
        float amount = currentContainer.MaxCapacity;
        GameManager.Instance.RecordDrawWater(amount, waterQuality, staminaCost);

        // 一定時間後に水を止める（flowDurationが非常に大きい場合は実質的に無制限）
        if (flowDuration < 999999f)
        {
            Invoke(nameof(StopWaterFlow), flowDuration);
        }
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

