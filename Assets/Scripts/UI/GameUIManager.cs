using TMPro;
using UnityEngine;

/// <summary>
/// ゲームUI全体の制御（HUD + リザルト画面）
/// </summary>
public class GameUIManager : MonoBehaviour
{
    public enum LanguageOption
    {
        Japanese = 0,
        English = 1
    }

    [Header("HUD設定")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private TextMeshProUGUI taskProgressText;
    [SerializeField] private TextMeshProUGUI waterVolumeText;
    [SerializeField] private TextMeshProUGUI waterQualityText;
    [SerializeField] private TextMeshProUGUI staminaText;
    [Tooltip("水量表示モード: true=現在保持量, false=累積使用量")]
    [SerializeField] private bool showCurrentVolume = true;

    [Header("リザルト画面設定")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private TextMeshProUGUI actionDetailsText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("ローカライゼーション設定")]
    [SerializeField] private LanguageOption language = LanguageOption.Japanese;

    private void Start()
    {
        // ゲーム終了イベントを購読
        GameManager.Instance.OnGameEnded += ShowResults;
        resultPanel.SetActive(false);
    }

    private void Update()
    {
        // HUD更新（GameDataから直接読み込み）
        var data = GameManager.Instance.Data;
        var history = data.History;
        var loc = GetLocalizationStrings();
        float totalWaterUsed = history.WaterUsedForFarming +
                               history.WaterUsedForDrinking +
                               history.WaterUsedForWashing +
                               history.WaterWasted;

        taskProgressText.text = $"{loc.TaskLabel}: {data.History.TotalTasksCompleted}/{data.MaxTasks}";

        // 水量表示：現在保持量 または 累積使用量
        if (showCurrentVolume)
        {
            waterVolumeText.text = $"{loc.WaterVolumeLabel}: {data.WaterVolume:F0}L";
        }
        else
        {
            waterVolumeText.text = $"{loc.WaterUsedLabel}: {totalWaterUsed:F0}L";
        }

        waterQualityText.text = $"{loc.WaterQualityLabel}: {data.WaterQuality:F0}";
        staminaText.text = $"{loc.StaminaLabel}: {data.Stamina:F0}";

        // 色変化
        waterQualityText.color = data.WaterQuality >= 80f ? Color.green : Color.red;
        staminaText.color = data.Stamina <= 20f ? Color.red : Color.white;
    }

    // === リザルト画面 ===

    private void ShowResults()
    {
        resultPanel.SetActive(true);

        var history = GameManager.Instance.Data.History;
        var loc = GetLocalizationStrings();

        // サマリー表示
        float totalWaterUsed = history.WaterUsedForFarming + history.WaterUsedForDrinking + history.WaterUsedForWashing;
        summaryText.text = $"{loc.SummaryTotalTasks}: {history.TotalTasksCompleted}\n" +
                          $"{loc.SummaryWaterDrawn}: {history.WaterDrawn:F1}L\n" +
                          $"{loc.SummaryWaterUsed}: {totalWaterUsed:F1}L\n" +
                          $"{loc.SummaryWaterPolluted}: {history.WaterPolluted:F1}L\n" +
                          $"{loc.SummaryStaminaSpent}: {history.StaminaSpent:F1}\n" +
                          $"{loc.SummaryStaminaRecovered}: {history.StaminaRecovered:F1}";

        // 行動内訳
        string details = $"{loc.ActionDraw}: {history.DrawCount}\n" +
                        $"{loc.ActionFarming}: {history.FarmCount}\n" +
                        $"{loc.ActionDrinking}: {history.DrinkCount}\n" +
                        $"{loc.ActionLaundry}: {history.WashCount}\n" +
                        $"{loc.ActionWaste}: {history.WasteCount}";
        actionDetailsText.text = details;

        // スコア
        float hygiene = GameManager.Instance.CalculateHygiene();
        float efficiency = GameManager.Instance.CalculateEfficiency();
        scoreText.text = $"{loc.ScoreHygiene}: {hygiene:F1}\n{loc.ScoreEfficiency}: {efficiency:F1}";
    }

    public void OnRestartButton()
    {
        resultPanel.SetActive(false);
        GameManager.Instance.Data.Reset();  // データリセットのみ
    }

    public void SetLanguage(LanguageOption newLanguage)
    {
        if (language == newLanguage) return;
        language = newLanguage;
        if (resultPanel.activeSelf)
        {
            ShowResults();
        }
    }

    public void ApplyRuntimeOptions(GameUIRuntimeOptions options)
    {
        SetLanguage(options.Language);
    }

    private LocalizationStrings GetLocalizationStrings()
    {
        switch (language)
        {
            case LanguageOption.English:
                return new LocalizationStrings
                {
                    TaskLabel = "Tasks",
                    WaterVolumeLabel = "Water Volume",
                    WaterUsedLabel = "Water Used",
                    WaterQualityLabel = "Water Quality",
                    StaminaLabel = "Stamina",
                    SummaryTotalTasks = "Total Tasks",
                    SummaryWaterDrawn = "Water Collected",
                    SummaryWaterUsed = "Water Used",
                    SummaryWaterPolluted = "Water Polluted",
                    SummaryStaminaSpent = "Stamina Spent",
                    SummaryStaminaRecovered = "Stamina Recovered",
                    ActionDraw = "Water Draws",
                    ActionFarming = "Field Work",
                    ActionDrinking = "Drinks",
                    ActionLaundry = "Laundry",
                    ActionWaste = "Disposals",
                    ScoreHygiene = "Hygiene Score",
                    ScoreEfficiency = "Efficiency Score"
                };
            default:
                return new LocalizationStrings
                {
                    TaskLabel = "タスク",
                    WaterVolumeLabel = "保持水量",
                    WaterUsedLabel = "使用した水量",
                    WaterQualityLabel = "水質",
                    StaminaLabel = "体力",
                    SummaryTotalTasks = "総タスク数",
                    SummaryWaterDrawn = "取得した水量",
                    SummaryWaterUsed = "使用した水量",
                    SummaryWaterPolluted = "汚れた水量",
                    SummaryStaminaSpent = "体力消費",
                    SummaryStaminaRecovered = "体力回復",
                    ActionDraw = "水汲み",
                    ActionFarming = "畑作業",
                    ActionDrinking = "飲用",
                    ActionLaundry = "洗濯",
                    ActionWaste = "廃棄",
                    ScoreHygiene = "衛生スコア",
                    ScoreEfficiency = "効率スコア"
                };
        }
    }

    private struct LocalizationStrings
    {
        public string TaskLabel;
        public string WaterVolumeLabel;
        public string WaterUsedLabel;
        public string WaterQualityLabel;
        public string StaminaLabel;
        public string SummaryTotalTasks;
        public string SummaryWaterDrawn;
        public string SummaryWaterUsed;
        public string SummaryWaterPolluted;
        public string SummaryStaminaSpent;
        public string SummaryStaminaRecovered;
        public string ActionDraw;
        public string ActionFarming;
        public string ActionDrinking;
        public string ActionLaundry;
        public string ActionWaste;
        public string ScoreHygiene;
        public string ScoreEfficiency;
    }

    [System.Serializable]
    public struct GameUIRuntimeOptions
    {
        public LanguageOption Language;
    }
}
