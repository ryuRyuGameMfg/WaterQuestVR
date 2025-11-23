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

    public void RecordDrawWater(float amount, float quality)
    {
        Data.WaterVolume += amount;
        Data.WaterQuality = Mathf.Max(Data.WaterQuality, quality); // 水質向上
        Data.History.WaterDrawn += amount;
        Data.History.DrawCount++;

        Debug.Log($"[GameManager] 水を汲みました。水量: +{amount:F0}L、現在の総水量: {Data.WaterVolume:F0}L、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordFarming(float amount)
    {
        Data.WaterVolume -= amount;
        Data.WaterQuality -= 5f; // 水質低下
        Data.History.WaterUsedForFarming += amount;
        Data.History.FarmCount++;

        Debug.Log($"[GameManager] 農業タスクを完了しました。消費水量: {amount:F0}L、水質: {Data.WaterQuality:F0}、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordDrinking(float amount, float quality)
    {
        Data.WaterVolume -= amount;
        bool isSafe = quality >= SAFE_QUALITY_THRESHOLD;

        // 体力変化
        float staminaChange = isSafe ? 10f : -10f;
        Data.Stamina += staminaChange;
        Data.Stamina = Mathf.Clamp(Data.Stamina, 0f, 100f);

        Data.History.WaterUsedForDrinking += amount;
        Data.History.DrinkCount++;

        Debug.Log($"[GameManager] 飲用タスクを完了しました。消費水量: {amount:F0}L、体力変化: {(staminaChange > 0 ? "+" : "")}{staminaChange:F0}、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordWaste(float amount)
    {
        Data.WaterVolume -= amount;
        Data.WaterQuality -= 10f; // 環境汚染
        Data.History.WaterWasted += amount;
        Data.History.WaterPolluted += amount;
        Data.History.WasteCount++;

        Debug.Log($"[GameManager] 水を廃棄しました。廃棄水量: {amount:F0}L、環境汚染: -10、タスク数: {Data.History.TotalTasksCompleted}/{Data.MaxTasks}");

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    // === スコア計算 ===

    public float CalculateHygiene()
    {
        float totalUsed = Data.History.WaterUsedForDrinking + Data.History.WaterUsedForFarming;
        float unsafeUsed = 0f;  // 簡易版では0（詳細版では水質80未満での飲用量を記録）

        if (totalUsed == 0) return 100f;
        return (1f - (unsafeUsed / totalUsed)) * 100f;
    }

    public float CalculateEfficiency()
    {
        float totalDrawn = Data.History.WaterDrawn;
        float totalUsed = Data.History.WaterUsedForDrinking + Data.History.WaterUsedForFarming;

        if (totalDrawn == 0) return 0f;
        return (totalUsed / totalDrawn) * 100f;
    }
}
