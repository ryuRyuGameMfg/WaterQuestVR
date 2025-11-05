using TMPro;
using UnityEngine;

/// <summary>
/// ゲームUI全体の制御（HUD + リザルト画面）
/// </summary>
public class GameUIManager : MonoBehaviour
{
    [Header("HUD Settings")]
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Transform playerHead; // VRカメラ（OVRCameraRig）
    [SerializeField] private TextMeshProUGUI taskProgressText;
    [SerializeField] private TextMeshProUGUI waterVolumeText;
    [SerializeField] private TextMeshProUGUI waterQualityText;
    [SerializeField] private TextMeshProUGUI staminaText;

    [Header("Result UI Settings")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private TextMeshProUGUI actionDetailsText;
    [SerializeField] private TextMeshProUGUI scoreText;

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

        taskProgressText.text = $"タスク: {data.History.TotalTasksCompleted}/{data.MaxTasks}";
        waterVolumeText.text = $"水量: {data.WaterVolume:F0}/100";
        waterQualityText.text = $"水質: {data.WaterQuality:F0}/100";
        staminaText.text = $"体力: {data.Stamina:F0}/100";

        // 色変化
        waterQualityText.color = data.WaterQuality >= 80f ? Color.green : Color.red;
        staminaText.color = data.Stamina <= 20f ? Color.red : Color.white;
    }

    private void LateUpdate()
    {
        // HUDをプレイヤーの視界に追従（World Space Canvas）
        if (playerHead != null)
        {
            hudCanvas.transform.position = playerHead.position +
                                           playerHead.forward * 2f +
                                           playerHead.up * 0.5f;
            hudCanvas.transform.LookAt(playerHead);
        }
    }

    // === リザルト画面 ===

    private void ShowResults()
    {
        resultPanel.SetActive(true);

        var history = GameManager.Instance.Data.History;

        // サマリー表示
        summaryText.text = $"総タスク数: {history.TotalTasksCompleted}\n" +
                          $"総水消費量: {history.WaterDrawn:F1}L\n" +
                          $"総水汚染量: {history.WaterPolluted:F1}L";

        // 行動内訳
        string details = $"水汲み: {history.DrawCount}回\n" +
                        $"畑作業: {history.FarmCount}回\n" +
                        $"飲用: {history.DrinkCount}回\n" +
                        $"廃棄: {history.WasteCount}回";
        actionDetailsText.text = details;

        // スコア
        float hygiene = GameManager.Instance.CalculateHygiene();
        float efficiency = GameManager.Instance.CalculateEfficiency();
        scoreText.text = $"衛生スコア: {hygiene:F1}\n効率スコア: {efficiency:F1}";
    }

    public void OnRestartButton()
    {
        resultPanel.SetActive(false);
        GameManager.Instance.Data.Reset();  // データリセットのみ
    }
}
