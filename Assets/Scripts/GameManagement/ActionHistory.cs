using System;

/// <summary>
/// ゲームセッション全体の行動履歴
/// </summary>
[Serializable]
public class ActionHistory
{
    // 水量トラッキング
    public float WaterDrawn;           // 汲んだ水量
    public float WaterUsedForFarming;  // 畑に使った水量
    public float WaterUsedForDrinking; // 飲んだ水量
    public float WaterUsedForWashing;  // 洗濯に使った水量（将来用）
    public float WaterWasted;          // 捨てた水量
    public float WaterPolluted;        // 汚染された水量
    public float UnsafeDrinkingAmount; // 水質80未満で飲用に使用した水量（衛生スコア計算用）

    // 体力トラッキング
    public float StaminaSpent;         // 消費した体力
    public float StaminaRecovered;     // 回復した体力

    // 行動回数
    public int DrawCount;
    public int FarmCount;
    public int DrinkCount;
    public int WashCount;
    public int WasteCount;

    // 総タスク数（自動計算）
    public int TotalTasksCompleted => DrawCount + FarmCount + DrinkCount + WashCount + WasteCount;
}
