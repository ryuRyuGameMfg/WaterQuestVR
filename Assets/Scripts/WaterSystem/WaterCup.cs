using UnityEngine;

/// <summary>
/// コップ（デフォルト容量10）
/// </summary>
public class WaterCup : WaterVessel
{
    private void Awake()
    {
        // デフォルト値（インスペクターで上書き可能）
        if (maxCapacity == 0f)
        {
            maxCapacity = 10f;
        }
    }

    protected override void OnTiltedOutsideTask()
    {
        // タスク場所外で傾けたら自動廃棄
        // （実際の廃棄処理はDrinkingPointで制御するため、ここでは何もしない）
    }
}
