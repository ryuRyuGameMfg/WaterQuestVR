using UnityEngine;

/// <summary>
/// 畑（水をまく）
/// </summary>
public class Field : MonoBehaviour
{
    [Header("Field Settings")]
    [SerializeField] private float waterConsumption = 50f;   // 消費する水量
    [SerializeField] private float qualityDecrease = 5f;     // 水質低下
    [SerializeField] private float staminaCost = 15f;        // 体力コスト

    private WaterVessel currentContainer = null;
    private bool hasUsedWater = false; // 1回のみ実行

    private void OnTriggerEnter(Collider other)
    {
        // バケツが範囲内に入った
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container != null && container is WaterBucket)
        {
            currentContainer = container;
            hasUsedWater = false; // リセット
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // バケツが範囲外に出た
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container == currentContainer)
        {
            currentContainer = null;
        }
    }

    private void Update()
    {
        // 範囲内でバケツを傾けたら水やり実行
        if (currentContainer != null &&
            currentContainer.IsFull &&
            currentContainer.IsPouringAngle() &&
            !hasUsedWater)
        {
            PourWater(currentContainer);
        }
    }

    private void PourWater(WaterVessel container)
    {
        // 水を空にする
        container.EmptyWater();

        // GameManagerに記録
        GameManager.Instance.RecordFarming(waterConsumption);
        GameManager.Instance.Data.Stamina -= staminaCost;

        hasUsedWater = true; // 1回のみ実行
    }
}
