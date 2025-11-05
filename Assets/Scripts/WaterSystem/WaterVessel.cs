using UnityEngine;

/// <summary>
/// 水器具の抽象基底クラス（バケツ・コップ共通機能）
/// </summary>
public abstract class WaterVessel : MonoBehaviour
{
    [Header("Container Settings")]
    [SerializeField] protected float maxCapacity = 80f;           // 最大容量
    [SerializeField] protected float waterQuality = 100f;         // 水質
    [SerializeField] protected float pourAngleThreshold = 45f;    // 傾き閾値

    // 2値管理（満タン/空）
    protected bool isFull = false;

    // プロパティ
    public bool IsFull => isFull;
    public float MaxCapacity => maxCapacity;
    public float WaterQuality => waterQuality;

    /// <summary>
    /// 傾き判定（角度ベース）
    /// </summary>
    public virtual bool IsPouringAngle()
    {
        float angleX = Mathf.Abs(transform.eulerAngles.x);
        float angleZ = Mathf.Abs(transform.eulerAngles.z);

        // 180度を超える場合は補正
        if (angleX > 180f) angleX = 360f - angleX;
        if (angleZ > 180f) angleZ = 360f - angleZ;

        return angleX > pourAngleThreshold || angleZ > pourAngleThreshold;
    }

    /// <summary>
    /// 水を満タンにする
    /// </summary>
    public virtual void FillWater(float quality)
    {
        isFull = true;
        waterQuality = quality;
    }

    /// <summary>
    /// 水を空にする
    /// </summary>
    public virtual void EmptyWater()
    {
        if (!isFull) return; // すでに空なら何もしない

        isFull = false;
        waterQuality = 0f;
    }

    protected virtual void Update()
    {
        // タスク場所にいない時に傾けたら自動廃棄
        if (isFull && IsPouringAngle())
        {
            OnTiltedOutsideTask();
        }
    }

    /// <summary>
    /// タスク場所外で傾けられた時の処理（自動廃棄）
    /// </summary>
    protected virtual void OnTiltedOutsideTask()
    {
        // サブクラスで具体的な廃棄処理を実装
        // または、タスク場所でのみ無効化するフラグを持つ
    }
}
