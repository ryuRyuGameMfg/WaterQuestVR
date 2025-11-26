using System;
using UnityEngine;

/// <summary>
/// ゲーム全体の制御（シングルトン）
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // データ
    public GameData Data = new GameData();

    // 定数
    public const float SAFE_QUALITY_THRESHOLD = 80f;

    // イベント
    public event Action OnTaskCompleted;  // 各タスク完了時
    public event Action OnGameEnded;       // ゲーム終了時

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ゲーム開始時にデータをリセット（VRスポーン時に自動的に開始）
        Data.Reset();
    }

    // === タスク完了チェック（自動終了判定） ===

    private void CheckGameCompletion()
    {
        // タスク数が設定値に達したら自動的にゲーム終了
        if (Data.History.TotalTasksCompleted >= Data.MaxTasks)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        Debug.Log($"[GameManager] ゲーム終了！総タスク数: {Data.History.TotalTasksCompleted}、衛生スコア: {CalculateHygiene():F1}、効率スコア: {CalculateEfficiency():F1}");
        OnGameEnded?.Invoke();
        // ResultUI が自動的に表示される（OnGameEnded イベントを購読）
    }

    // === 行動記録（各アクション実行時に呼ばれる） ===

    public void RecordDrawWater(float amount, float quality, float staminaCost = 0f)
    {
        Data.WaterVolume = Mathf.Max(0f, Data.WaterVolume + amount);
        Data.WaterQuality = Mathf.Clamp(Mathf.Max(Data.WaterQuality, quality), 0f, 100f); // 水質向上
        SpendStamina(staminaCost);

        Data.History.WaterDrawn += amount;
        Data.History.DrawCount++;

        Debug.Log($"[GameManager] 水を汲みました。水量: +{amount:F0}L、現在の総水量: {Data.WaterVolume:F0}L、体力消費: {staminaCost:F0}、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordFarming(float amount, float qualityDecrease, float staminaCost)
    {
        Data.WaterVolume = Mathf.Max(0f, Data.WaterVolume - amount);
        DecreaseWaterQuality(qualityDecrease);
        SpendStamina(staminaCost);

        Data.History.WaterUsedForFarming += amount;
        Data.History.WaterPolluted += amount;
        Data.History.FarmCount++;

        Debug.Log($"[GameManager] 農業タスクを完了しました。消費水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordDrinking(float amount, float quality, float safeStaminaGain, float unsafeStaminaLoss)
    {
        Data.WaterVolume = Mathf.Max(0f, Data.WaterVolume - amount);
        bool isSafe = quality >= SAFE_QUALITY_THRESHOLD;

        if (isSafe)
        {
            RecoverStamina(safeStaminaGain);
        }
        else
        {
            SpendStamina(unsafeStaminaLoss);
        }

        Data.History.WaterUsedForDrinking += amount;
        Data.History.DrinkCount++;

        // 水質80未満での飲用量を記録（衛生スコア計算用）
        if (!isSafe)
        {
            Data.History.UnsafeDrinkingAmount += amount;
        }

        float staminaDelta = isSafe ? safeStaminaGain : -unsafeStaminaLoss;
        Debug.Log($"[GameManager] 飲用タスクを完了しました。消費水量: {amount:F0}L、水質: {quality:F0}、体力変化: {(staminaDelta >= 0 ? "+" : "")}{staminaDelta:F0}、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordLaundry(float amount, float qualityDecrease, float staminaCost)
    {
        Data.WaterVolume = Mathf.Max(0f, Data.WaterVolume - amount);
        DecreaseWaterQuality(qualityDecrease);
        SpendStamina(staminaCost);

        Data.History.WaterUsedForWashing += amount;
        Data.History.WaterPolluted += amount;
        Data.History.WashCount++;

        Debug.Log($"[GameManager] 洗濯タスクを完了しました。消費水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordWaste(float amount, float qualityDecrease, float staminaCost)
    {
        Data.WaterVolume = Mathf.Max(0f, Data.WaterVolume - amount);
        DecreaseWaterQuality(qualityDecrease);
        SpendStamina(staminaCost);

        Data.History.WaterWasted += amount;
        Data.History.WaterPolluted += amount;
        Data.History.WasteCount++;

        Debug.Log($"[GameManager] 水を廃棄しました。廃棄水量: {amount:F0}L、水質低下: {qualityDecrease:F0}、体力消費: {staminaCost:F0}、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    // === スコア計算 ===

    public float CalculateHygiene()
    {
        // 全使用量：飲用・農業・廃棄を含めた水使用総量（仕様書に基づく）
        float totalUsed = Data.History.WaterUsedForDrinking +
                          Data.History.WaterUsedForFarming +
                          Data.History.WaterWasted;

        // 不適切用途の使用量：水質が80未満の状態で飲用に使用した水量の合計
        float unsafeUsed = Data.History.UnsafeDrinkingAmount;

        if (totalUsed == 0) return 100f;
        return (1f - (unsafeUsed / totalUsed)) * 100f;
    }

    public float CalculateEfficiency()
    {
        float totalDrawn = Data.History.WaterDrawn;
        float totalUsed = Data.History.WaterUsedForDrinking + Data.History.WaterUsedForFarming + Data.History.WaterUsedForWashing;

        if (totalDrawn == 0) return 0f;
        return (totalUsed / totalDrawn) * 100f;
    }

    private void DecreaseWaterQuality(float amount)
    {
        if (amount <= 0f) return;
        Data.WaterQuality = Mathf.Clamp(Data.WaterQuality - amount, 0f, 100f);
    }

    private void SpendStamina(float amount)
    {
        if (amount <= 0f) return;
        Data.Stamina = Mathf.Clamp(Data.Stamina - amount, 0f, 100f);
        Data.History.StaminaSpent += amount;
    }

    private void RecoverStamina(float amount)
    {
        if (amount <= 0f) return;
        Data.Stamina = Mathf.Clamp(Data.Stamina + amount, 0f, 100f);
        Data.History.StaminaRecovered += amount;
    }
}
