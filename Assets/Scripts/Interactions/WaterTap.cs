using UnityEngine;

/// <summary>
/// 水道口（蛇口）- 水を汲む
/// </summary>
public class WaterTap : MonoBehaviour
{
    [Header("Water Effect")]
    [SerializeField] private GameObject obiWaterEffect;      // Obiの水エフェクト

    [Header("Water Settings")]
    [SerializeField] private float waterQuality = 100f;      // 汲める水の水質
    [SerializeField] private float staminaCost = 10f;        // 体力コスト

    [Header("Trigger Settings")]
    [SerializeField] private bool useButtonTrigger = true;   // ボタン押下で水を出す
    [SerializeField] private bool useCollisionTrigger = false; // 器具接触で水を出す

    private WaterVessel currentContainer = null;
    private bool isWaterFlowing = false;

    private void OnTriggerEnter(Collider other)
    {
        // 器具が水道口付近に入った
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container != null && !container.IsFull)
        {
            currentContainer = container;

            // 衝突トリガーモードの場合、自動的に水を出す
            if (useCollisionTrigger)
            {
                StartWaterFlow();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container == currentContainer)
        {
            StopWaterFlow();
            currentContainer = null;
        }
    }

    private void Update()
    {
        // ボタントリガーモード: OVRInput でボタン検出
        if (useButtonTrigger && currentContainer != null && !isWaterFlowing)
        {
            // Aボタン or ピンチジェスチャー
            if (OVRInput.GetDown(OVRInput.Button.One)) // Aボタン
            {
                StartWaterFlow();
            }
        }
    }

    private void StartWaterFlow()
    {
        if (currentContainer == null || currentContainer.IsFull) return;

        // Obiエフェクトを表示（水が出る）
        if (obiWaterEffect != null)
        {
            obiWaterEffect.SetActive(true);
        }

        isWaterFlowing = true;

        // 器具に水を満タンにする
        currentContainer.FillWater(waterQuality);

        // GameManagerに記録
        float amount = currentContainer.MaxCapacity;
        GameManager.Instance.RecordDrawWater(amount, waterQuality);
        GameManager.Instance.Data.Stamina -= staminaCost;

        // 一定時間後に水を止める
        Invoke(nameof(StopWaterFlow), 2f);
    }

    private void StopWaterFlow()
    {
        // Obiエフェクトを非表示（水が止まる）
        if (obiWaterEffect != null)
        {
            obiWaterEffect.SetActive(false);
        }

        isWaterFlowing = false;
    }
}
