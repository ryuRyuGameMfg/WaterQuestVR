using UnityEngine;

/// <summary>
/// バケツ（デフォルト容量80）
/// </summary>
public class WaterBucket : WaterVessel
{
    private void Awake()
    {
        // デフォルト値（インスペクターで上書き可能）
        if (maxCapacity == 0f)
        {
            maxCapacity = 80f;
        }
    }

    protected override void OnTiltedOutsideTask()
    {
        // タスク場所外で傾けたら自動廃棄
        // （実際の廃棄処理はField, DrinkingPointで制御するため、ここでは何もしない）
    }
}
