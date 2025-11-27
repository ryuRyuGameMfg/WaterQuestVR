using UnityEngine;

/// <summary>
/// 水を受けるもの（畑、飲用ポイントなど）
/// 器具から水を空にする処理
/// </summary>
public abstract class WaterReceiver : WaterInteractionBase
{
    [Header("水消費設定")]
    [SerializeField] protected float waterConsumption = 50f;   // 消費する水量
    [SerializeField] protected float staminaCost = 15f;        // 体力コスト

    [Header("1回実行設定")]
    [Tooltip("1回のみ実行するか")]
    [SerializeField] protected bool oneTimeExecution = true;

    [Header("クールダウン設定")]
    [Tooltip("タスク実行後のクールダウン時間（秒）。連続実行を防ぐために使用")]
    [Min(0f)]
    [SerializeField] protected float cooldownTime = 0.5f;

    [Header("視覚的フィードバック")]
    [Tooltip("マテリアルを変更するRenderer（自動取得も可能）")]
    [SerializeField] protected Renderer targetRenderer;
    [Tooltip("タスク実行前のマテリアル（通常の色）")]
    [SerializeField] protected Material beforeMaterial;
    [Tooltip("タスク実行後のマテリアル（完了後の色）")]
    [SerializeField] protected Material afterMaterial;

    protected bool hasExecuted = false;
    protected bool hasBeenCompleted = false;
    private float lastExecutionTime = -1f;

    protected override void Awake()
    {
        base.Awake();

        // デフォルト設定（インスペクターで変更可能）
        requiresFull = true; // 満タンの器具が必要
        // conditionType はインスペクターで設定（デフォルト: TiltDetection）

        // Rendererの自動取得
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<Renderer>();
            }
        }

        // 初期マテリアルの設定
        if (targetRenderer != null && beforeMaterial != null)
        {
            targetRenderer.material = beforeMaterial;
        }
    }

    protected override void OnContainerEntered(WaterVessel container)
    {
        // 1回実行制限のリセット
        if (oneTimeExecution)
        {
            hasExecuted = false;
        }
    }

    protected override void OnContainerExited(WaterVessel container)
    {
        // 範囲外に出たら実行フラグをリセット
        hasExecuted = false;
    }

    protected override bool CheckCondition()
    {
        // クールダウン中のチェック
        if (Time.time - lastExecutionTime < cooldownTime)
        {
            return false;
        }

        // 1回実行制限のチェック
        if (oneTimeExecution && hasExecuted)
        {
            Debug.Log($"[{gameObject.name}] CheckCondition: 既に実行済みです（hasExecuted=true, oneTimeExecution=true）");
            return false;
        }

        bool result = base.CheckCondition();
        if (result)
        {
            Debug.Log($"[{gameObject.name}] CheckCondition: 条件が満たされました（hasExecuted={hasExecuted}, oneTimeExecution={oneTimeExecution}）");
        }
        return result;
    }

    protected override void ExecuteTask()
    {
        if (currentContainer == null) return;

        // 満タンでない場合はタスクを実行しない
        // （WaterVessel.Update()で別の場所で傾けて空になった器具が範囲内に入った場合を防ぐ）
        if (!currentContainer.IsFull)
        {
            Debug.Log($"[{gameObject.name}] ExecuteTask: 器具が既に空です。タスクを実行しません。");
            return;
        }

        // 1回実行制限のチェック
        if (oneTimeExecution && hasExecuted)
        {
            Debug.Log($"[{gameObject.name}] ExecuteTask: 既に実行済みです。");
            return;
        }

        // 満タンの器具でタスクを実行
        ConsumeWater();
        hasExecuted = true;
        isExecuting = true;
        lastExecutionTime = Time.time; // クールダウンタイマーを更新

        // 視覚的フィードバック（マテリアル変更）
        if (!hasBeenCompleted)
        {
            UpdateMaterial();
            hasBeenCompleted = true;
        }
    }

    /// <summary>
    /// 水を消費する処理（サブクラスで実装）
    /// </summary>
    protected abstract void ConsumeWater();

    /// <summary>
    /// マテリアルを変更して視覚的フィードバックを提供
    /// </summary>
    protected virtual void UpdateMaterial()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Target Rendererが設定されていません。マテリアルを更新できません。");
            return;
        }

        if (afterMaterial == null)
        {
            Debug.LogWarning($"[{gameObject.name}] After Materialが設定されていません。");
            return;
        }

        targetRenderer.material = afterMaterial;
        Debug.Log($"[{gameObject.name}] マテリアルを変更しました（タスク完了後）");
    }
}

