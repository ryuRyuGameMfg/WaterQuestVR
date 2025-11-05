using System;
using UnityEngine;

/// <summary>
/// ゲーム全体のデータ保持
/// </summary>
[Serializable]
public class GameData
{
    // タスク設定（インスペクターで設定可能）
    [Range(3, 10)] public int MaxTasks = 5;  // ゲーム終了までのタスク数

    // 現在のパラメータ
    [Range(0, 100)] public float WaterVolume = 0f;
    [Range(0, 100)] public float WaterQuality = 100f;
    [Range(0, 100)] public float Stamina = 100f;

    // ゲームセッション全体の履歴
    public ActionHistory History = new ActionHistory();

    // リセット
    public void Reset()
    {
        WaterVolume = 0f;
        WaterQuality = 100f;
        Stamina = 100f;
        History = new ActionHistory();
    }
}
