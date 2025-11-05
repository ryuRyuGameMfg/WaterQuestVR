using UnityEngine;

/// <summary>
/// 飲用ポイント（口元）
/// </summary>
public class DrinkingPoint : MonoBehaviour
{
    [Header("Drinking Settings")]
    [SerializeField] private float waterConsumption = 10f;        // 消費する水量
    [SerializeField] private float safeStaminaGain = 10f;         // 安全な水の体力回復
    [SerializeField] private float unsafeStaminaLoss = 10f;       // 汚染水の体力減少
    [SerializeField] private float safeQualityThreshold = 80f;    // 安全閾値

    private WaterVessel currentContainer = null;
    private bool hasDrunk = false; // 1回のみ実行

    private void OnTriggerEnter(Collider other)
    {
        // コップが範囲内に入った
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container != null && container is WaterCup)
        {
            currentContainer = container;
            hasDrunk = false; // リセット
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // コップが範囲外に出た
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container == currentContainer)
        {
            currentContainer = null;
        }
    }

    private void Update()
    {
        // 範囲内でコップを傾けたら飲用実行
        if (currentContainer != null &&
            currentContainer.IsFull &&
            currentContainer.IsPouringAngle() &&
            !hasDrunk)
        {
            Drink(currentContainer);
        }
    }

    private void Drink(WaterVessel container)
    {
        // 水質チェック
        float quality = container.WaterQuality;

        // 水を空にする
        container.EmptyWater();

        // GameManagerに記録（体力変化も含む）
        GameManager.Instance.RecordDrinking(waterConsumption, quality);

        hasDrunk = true; // 1回のみ実行
    }
}
