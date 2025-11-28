using UnityEngine;

/// <summary>
/// 水捨てエリア（範囲内で器具を傾けたら水を捨てる）
/// WaterVesselから水を注がれた時に廃棄タスクを記録
/// </summary>
public class WaterDisposalZone : MonoBehaviour, IWaterReceiver
{
    [Header("廃棄設定")]
    [SerializeField] private float qualityDecrease = 10f;  // 環境汚染
    [SerializeField] private float staminaCost = 2f;       // 体力コスト

    // IWaterReceiver実装（廃棄エリアは常に水を受け取れる）
    public bool CanReceiveWater => true;

    // 現在範囲内にいる器具
    private WaterVessel currentVessel = null;

    private void OnTriggerEnter(Collider other)
    {
        // 器具（バケツまたはコップ）が範囲内に入った
        WaterVessel vessel = other.GetComponent<WaterVessel>();
        if (vessel == null) return;

        currentVessel = vessel;

        if (vessel.HasWater)
        {
            // WaterVesselに自分を登録
            vessel.RegisterReceiver(this);
            Debug.Log($"[{gameObject.name}] 器具が廃棄エリアに入りました（水あり）: {vessel.gameObject.name}");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] 器具が廃棄エリアに入りました（水なし）: {vessel.gameObject.name}");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 水を汲んだ後に範囲内で傾けるケースに対応
        WaterVessel vessel = other.GetComponent<WaterVessel>();
        if (vessel == null || vessel != currentVessel) return;

        if (vessel.HasWater)
        {
            vessel.RegisterReceiver(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 器具が範囲外に出た
        WaterVessel vessel = other.GetComponent<WaterVessel>();
        if (vessel == null) return;

        if (vessel == currentVessel)
        {
            // WaterVesselから自分を登録解除
            vessel.UnregisterReceiver(this);
            currentVessel = null;
            Debug.Log($"[{gameObject.name}] 器具が廃棄エリアから出ました: {vessel.gameObject.name}");
        }
    }

    /// <summary>
    /// IWaterReceiver.ReceiveWater の実装
    /// WaterVesselから水を注がれた時に呼ばれる
    /// </summary>
    public bool ReceiveWater(float amount, float quality)
    {
        // ログ出力
        Debug.Log($"[{gameObject.name}] 水を捨てました。廃棄水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}");

        // GameManagerに記録
        GameManager.Instance.RecordWaste(amount, qualityDecrease, staminaCost);

        return true;
    }
}
