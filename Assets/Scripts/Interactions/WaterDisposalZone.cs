using UnityEngine;

/// <summary>
/// 水捨てエリア（範囲内で器具を傾けたら水を捨てる）
/// </summary>
public class WaterDisposalZone : MonoBehaviour
{
    [Header("Disposal Settings")]
    [SerializeField] private float qualityDecrease = 10f;  // 環境汚染
    [SerializeField] private float staminaCost = 2f;       // 体力コスト

    private WaterVessel currentContainer = null;
    private bool hasDisposed = false; // 1回のみ実行

    private void OnTriggerEnter(Collider other)
    {
        // 器具（バケツまたはコップ）が範囲内に入った
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container != null)
        {
            currentContainer = container;
            hasDisposed = false; // リセット
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 器具が範囲外に出た
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container == currentContainer)
        {
            currentContainer = null;
        }
    }

    private void Update()
    {
        // 範囲内で器具を傾けたら水を捨てる
        if (currentContainer != null &&
            currentContainer.IsFull &&
            currentContainer.IsPouringAngle() &&
            !hasDisposed)
        {
            DisposeWater(currentContainer);
        }
    }

    private void DisposeWater(WaterVessel container)
    {
        // 捨てる水量
        float amount = container.MaxCapacity;

        // 水を空にする
        container.EmptyWater();

        // ログ出力
        Debug.Log($"[{gameObject.name}] 水を捨てました。廃棄水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}");

        // GameManagerに記録
        GameManager.Instance.RecordWaste(amount, qualityDecrease, staminaCost);

        hasDisposed = true; // 1回のみ実行
    }
}
