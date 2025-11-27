using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// WaterVesselに水の出し入れ機能を追加するコンポーネント
/// コップやバケツ自体が水を出す側・受ける側の両方になれる
/// </summary>
[RequireComponent(typeof(WaterVessel))]
public class WaterVesselInteraction : WaterInteractionBase
{
    [Header("水を出す機能設定")]
    [Tooltip("水を出す機能を有効化")]
    [SerializeField] private bool enableWaterSource = true;

    [Header("水を受ける機能設定")]
    [Tooltip("水を受ける機能を有効化")]
    [SerializeField] private bool enableWaterReceiver = true;

    [Header("水の移動設定")]
    [FormerlySerializedAs("obiWaterEffect")]
    [SerializeField] private GameObject obiWaterObject;      // 水を出す時のObiエフェクト
    [SerializeField] private float transferAmount = 5f;       // 1回で移す水量
    [SerializeField] private float transferDuration = 1f;    // 移す時間（秒）

    private WaterVessel myVessel;
    private bool isTransferring = false;

    protected override void Awake()
    {
        base.Awake();

        myVessel = GetComponent<WaterVessel>();
        if (myVessel == null)
        {
            Debug.LogError($"{gameObject.name}: WaterVessel component is required!");
            enabled = false;
            return;
        }

        // デフォルト設定
        if (enableWaterSource)
        {
            requiresFull = false; // 変更: 部分的に水があれば移せるようにする
            conditionType = ConditionType.TiltDetection; // 傾きで水を出す
        }

        if (enableWaterReceiver)
        {
            // 水を受ける場合は空が必要（別途設定）
            // requiresFull = false; // これはOnTriggerEnterで動的に判定
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        // 自分のWaterVesselは除外
        WaterVessel otherVessel = other.GetComponent<WaterVessel>();
        if (otherVessel == null || otherVessel == myVessel) return;

        // 水を受ける側として機能する場合
        if (enableWaterReceiver && !myVessel.IsFull && otherVessel.CurrentWaterAmount > 0f)
        {
            // 空でない自分が水を持っている他の器具から水を受ける
            currentContainer = otherVessel;

            // CollisionDetectionの場合は即座に実行
            if (conditionType == ConditionType.CollisionDetection)
            {
                TransferWaterIn();
            }
            else
            {
                OnContainerEntered(otherVessel);
            }
        }
        // 水を出す側として機能する場合
        else if (enableWaterSource && myVessel.CurrentWaterAmount > 0f && !otherVessel.IsFull)
        {
            // 水を持っている自分が空でない他の器具に水を出す
            currentContainer = otherVessel;

            // CollisionDetectionの場合は即座に実行
            if (conditionType == ConditionType.CollisionDetection)
            {
                TransferWaterOut();
            }
            else
            {
                OnContainerEntered(otherVessel);
            }
        }
    }

    protected override void OnContainerEntered(WaterVessel container)
    {
        // 既にOnTriggerEnterで処理済み
    }

    protected override void ExecuteTask()
    {
        if (isTransferring) return;
        if (currentContainer == null) return;

        // 水を出す側として実行（部分的に水があれば移せる）
        if (enableWaterSource && myVessel.CurrentWaterAmount > 0f && !currentContainer.IsFull)
        {
            TransferWaterOut();
        }
        // 水を受ける側として実行（CollisionDetection以外の場合）
        else if (enableWaterReceiver && !myVessel.IsFull && currentContainer.CurrentWaterAmount > 0f)
        {
            TransferWaterIn();
        }
    }

    /// <summary>
    /// 水を出す（他の器具に移す）
    /// </summary>
    private void TransferWaterOut()
    {
        if (currentContainer == null || currentContainer.IsFull) return;
        if (myVessel.CurrentWaterAmount <= 0f) return; // 変更: IsFullからCurrentWaterAmountに変更（部分的に水があれば移せる）

        isTransferring = true;
        isExecuting = true;

        // Obiエフェクト表示
        if (obiWaterObject != null)
        {
            obiWaterObject.SetActive(true);
        }

        // 水を移す
        float transferableAmount = Mathf.Min(transferAmount, myVessel.CurrentWaterAmount);
        float quality = myVessel.WaterQuality;
        float actualTransferredAmount = currentContainer.FillWater(transferableAmount, quality);

        // 移した分だけ減らす
        if (actualTransferredAmount > 0f)
        {
            myVessel.ReduceWater(actualTransferredAmount);
        }

        // ログ出力
        Debug.Log($"[{gameObject.name}] {currentContainer.gameObject.name}に水を移しました。移動量: {actualTransferredAmount:F0}L、残り水量: {myVessel.CurrentWaterAmount:F0}L/{myVessel.MaxCapacity:F0}L、水質: {quality:F0}");

        // 一定時間後にエフェクト停止
        Invoke(nameof(StopTransfer), transferDuration);
    }

    /// <summary>
    /// 水を受ける（他の器具から移される）
    /// </summary>
    private void TransferWaterIn()
    {
        if (currentContainer == null || currentContainer.CurrentWaterAmount <= 0f) return;
        if (myVessel.IsFull) return;

        isTransferring = true;
        isExecuting = true;

        // 水を受ける（全量を移す）
        float transferableAmount = Mathf.Min(currentContainer.CurrentWaterAmount, myVessel.MaxCapacity - myVessel.CurrentWaterAmount);
        float quality = currentContainer.WaterQuality;
        float actualTransferredAmount = myVessel.FillWater(transferableAmount, quality);

        // 移した分だけ減らす
        if (actualTransferredAmount > 0f)
        {
            currentContainer.ReduceWater(actualTransferredAmount);
        }

        // ログ出力
        Debug.Log($"[{gameObject.name}] {currentContainer.gameObject.name}から水を受け取りました。移動量: {actualTransferredAmount:F0}L、現在の水量: {myVessel.CurrentWaterAmount:F0}L/{myVessel.MaxCapacity:F0}L、水質: {quality:F0}");

        // 処理完了
        isTransferring = false;
        isExecuting = false;
    }

    private void StopTransfer()
    {
        // Obiエフェクト停止
        if (obiWaterObject != null)
        {
            obiWaterObject.SetActive(false);
        }

        isTransferring = false;
        isExecuting = false;
    }

    protected override bool CheckCondition()
    {
        // CollisionDetectionの場合はOnTriggerEnterで既に実行済み
        if (conditionType == ConditionType.CollisionDetection) return false;

        // 水を出す側の場合
        if (enableWaterSource && myVessel.CurrentWaterAmount > 0f && currentContainer != null && !currentContainer.IsFull)
        {
            return base.CheckCondition();
        }
        // 水を受ける側の場合（CollisionDetection以外は通常通り）
        else if (enableWaterReceiver && !myVessel.IsFull && currentContainer != null && currentContainer.CurrentWaterAmount > 0f)
        {
            return base.CheckCondition();
        }

        return false;
    }
}

