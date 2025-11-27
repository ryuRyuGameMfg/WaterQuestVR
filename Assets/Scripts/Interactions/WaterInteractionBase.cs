using UnityEngine;

/// <summary>
/// 水関連インタラクションの基底クラス
/// 当たり判定と条件判定を統一管理
/// </summary>
public abstract class WaterInteractionBase : MonoBehaviour
{
    [Header("トリガー設定")]
    [Tooltip("当たり判定の範囲（Colliderが必要）")]
    [SerializeField] protected Collider triggerCollider;

    /// <summary>
    /// 条件判定のタイプ
    /// </summary>
    public enum ConditionType
    {
        ButtonPress = 0,        // ボタン押下
        TiltDetection = 1,      // 傾き検知
        CollisionDetection = 2  // 衝突検知
    }

    /// <summary>
    /// Meta Quest コントローラーのボタン選択
    /// </summary>
    public enum QuestButton
    {
        AButtonRight = 0,           // Aボタン右
        BButtonRight = 1,           // Bボタン右
        XButtonLeft = 2,            // Xボタン左
        YButtonLeft = 3,            // Yボタン左
        TriggerRight = 4,           // トリガー右
        TriggerLeft = 5,            // トリガー左
        GripRight = 6,              // グリップ右
        GripLeft = 7,               // グリップ左
        ThumbstickPressRight = 8,   // スティック押し込み右
        ThumbstickPressLeft = 9     // スティック押し込み左
    }

    [Header("条件設定")]
    [Tooltip("タスク実行の条件タイプ")]
    [SerializeField] protected ConditionType conditionType = ConditionType.ButtonPress;

    [Header("ボタン設定")]
    [Tooltip("ボタン条件の場合のボタン選択")]
    [SerializeField] protected QuestButton triggerButton = QuestButton.AButtonRight;

    [Header("傾き設定")]
    [Tooltip("傾き条件の場合の閾値（度）")]
    [SerializeField] protected float tiltAngleThreshold = 45f;

    /// <summary>
    /// 対象となる器具の型
    /// </summary>
    public enum AllowedVesselType
    {
        All = 0,        // すべて
        WaterBucket = 1, // バケツのみ
        WaterCup = 2     // コップのみ
    }

    [Header("容器フィルター")]
    [Tooltip("対象となる器具の型")]
    [SerializeField] protected AllowedVesselType allowedVesselTypeEnum = AllowedVesselType.All;
    [Tooltip("満タンが必要か（true = 満タン、false = 空）")]
    [SerializeField] protected bool requiresFull = false;

    // 内部で使用するType（enumから自動変換）
    protected System.Type allowedVesselType = null;

    // 現在範囲内の器具
    protected WaterVessel currentContainer = null;
    protected bool isExecuting = false;

    protected virtual void Awake()
    {
        Debug.Log($"[{gameObject.name}] WaterInteractionBase.Awake() が呼ばれました");

        // Trigger Colliderの設定（インスペクターで設定されていない場合は自動取得）
        if (triggerCollider == null)
        {
            // まず自分自身から取得を試みる
            triggerCollider = GetComponent<Collider>();

            // 見つからなければ子オブジェクトから検索
            if (triggerCollider == null)
            {
                triggerCollider = GetComponentInChildren<Collider>();
            }

            if (triggerCollider != null)
            {
                Debug.Log($"[{gameObject.name}] Trigger Collider を自動取得しました: {triggerCollider.gameObject.name}");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] Trigger Collider が見つかりません。インスペクターの「Trigger Collider」に対象の Collider（Is Trigger = true）を指定するか、GameObject に Collider コンポーネントを追加してください。");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Trigger Collider確認（インスペクター設定）: {triggerCollider.gameObject.name}, IsTrigger={triggerCollider.isTrigger}");
        }

        // Is Trigger の確認と警告
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] Trigger Collider の Is Trigger が false です。トリガー判定が必要なため true に設定してください。");
        }

        // enumからTypeに変換
        ConvertVesselTypeEnumToType();
        Debug.Log($"[{gameObject.name}] AllowedVesselType: {allowedVesselTypeEnum}, requiresFull: {requiresFull}");
    }

    protected virtual void Start()
    {
        Debug.Log($"[{gameObject.name}] WaterInteractionBase.Start() が呼ばれました。enabled={enabled}, gameObject.activeInHierarchy={gameObject.activeInHierarchy}");

        // Trigger Colliderの最終確認
        if (triggerCollider == null)
        {
            Debug.LogError($"[{gameObject.name}] ⚠️ Trigger Collider が指定されていません。OnTriggerEnter/Exit が動作しないため、必ず設定してください。");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] ✅ Trigger Collider確認: {triggerCollider.gameObject.name}, IsTrigger={triggerCollider.isTrigger} (true である必要があります)");
            if (!triggerCollider.isTrigger)
            {
                Debug.LogWarning($"[{gameObject.name}] ⚠️ Trigger Collider の Is Trigger が false のままです。インスペクターで true に変更してください。");
            }
        }

        // Rigidbodyの確認（OnTriggerEnterが呼ばれるために必要）
        Rigidbody rb = GetComponent<Rigidbody>();
        Rigidbody rbChild = GetComponentInChildren<Rigidbody>();
        if (rb == null && rbChild == null)
        {
            Debug.LogWarning($"[{gameObject.name}] ⚠️ Rigidbodyが見つかりません。OnTriggerEnterが呼ばれるには、少なくとも一方のオブジェクトにRigidbodyが必要です。");
        }
        else
        {
            Rigidbody foundRb = rb != null ? rb : rbChild;
            Debug.Log($"[{gameObject.name}] ✅ Rigidbody確認: {foundRb.gameObject.name}, IsKinematic={foundRb.isKinematic}");
        }
    }

    /// <summary>
    /// enumからTypeに変換
    /// </summary>
    private void ConvertVesselTypeEnumToType()
    {
        switch (allowedVesselTypeEnum)
        {
            case AllowedVesselType.All:
                allowedVesselType = null; // null = すべて
                break;
            case AllowedVesselType.WaterBucket:
                allowedVesselType = typeof(WaterBucket);
                break;
            case AllowedVesselType.WaterCup:
                allowedVesselType = typeof(WaterCup);
                break;
            default:
                allowedVesselType = null;
                break;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[{gameObject.name}] ===== OnTriggerEnter 呼ばれました =====");
        Debug.Log($"[{gameObject.name}] 衝突オブジェクト: {other.gameObject.name}");
        Debug.Log($"[{gameObject.name}] triggerCollider: {(triggerCollider != null ? triggerCollider.gameObject.name : "null")}");

        WaterVessel container = other.GetComponent<WaterVessel>();

        // 器具の型チェック
        if (container == null)
        {
            Debug.Log($"[{gameObject.name}] WaterVesselコンポーネントが見つかりません: {other.gameObject.name}");
            return;
        }

        Debug.Log($"[{gameObject.name}] WaterVessel検出: {container.GetType().Name}, IsFull={container.IsFull}");

        // 器具の型チェック
        if (allowedVesselType != null)
        {
            bool typeMatch = container.GetType().IsSubclassOf(allowedVesselType) || container.GetType() == allowedVesselType;
            if (!typeMatch)
            {
                Debug.Log($"[{gameObject.name}] 器具の型が一致しません。期待: {allowedVesselType.Name}, 実際: {container.GetType().Name}");
                return;
            }
        }

        // 満タン/空の条件チェック
        if (requiresFull && !container.IsFull)
        {
            Debug.Log($"[{gameObject.name}] 満タンが必要ですが、空です。");
            return;
        }
        if (!requiresFull && container.IsFull)
        {
            Debug.Log($"[{gameObject.name}] 空が必要ですが、満タンです。");
            return;
        }

        Debug.Log($"[{gameObject.name}] 条件チェック通過。currentContainerを設定します。");

        currentContainer = container;

        // CollisionDetectionの場合は、トリガーに入った瞬間に実行
        if (conditionType == ConditionType.CollisionDetection)
        {
            Debug.Log($"[{gameObject.name}] CollisionDetection: ExecuteTask()を実行します");
            ExecuteTask();
        }
        else
        {
            Debug.Log($"[{gameObject.name}] {conditionType}: OnContainerEntered()を呼び出します");
            OnContainerEntered(container);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        WaterVessel container = other.GetComponent<WaterVessel>();
        if (container == currentContainer)
        {
            OnContainerExited(container);
            currentContainer = null;
            isExecuting = false;
        }
    }

    protected virtual void Update()
    {
        // CollisionDetectionの場合はUpdateでチェックしない（OnTriggerEnterで実行済み）
        if (conditionType == ConditionType.CollisionDetection) return;

        if (currentContainer == null)
        {
            // デバッグログは毎フレーム出さない（1秒に1回程度）
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[{gameObject.name}] Update: currentContainerがnullです。範囲内に器具がありません。");
            }
            return;
        }

        if (isExecuting)
        {
            // デバッグログは毎フレーム出さない
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[{gameObject.name}] Update: 既に実行中です（isExecuting=true）");
            }
            return;
        }

        // 条件判定
        bool conditionMet = CheckCondition();

        if (conditionMet)
        {
            Debug.Log($"[{gameObject.name}] Update: 条件が満たされました。ExecuteTask()を実行します");
            ExecuteTask();
        }
    }

    /// <summary>
    /// 条件をチェック
    /// </summary>
    protected virtual bool CheckCondition()
    {
        switch (conditionType)
        {
            case ConditionType.ButtonPress:
                return CheckButtonInput();

            case ConditionType.TiltDetection:
                return CheckTiltAngle();

            case ConditionType.CollisionDetection:
                return true; // 衝突時点で既に実行

            default:
                return false;
        }
    }

    /// <summary>
    /// ボタン入力チェック
    /// </summary>
    protected virtual bool CheckButtonInput()
    {
        bool pressed = false;
        switch (triggerButton)
        {
            case QuestButton.AButtonRight:
                pressed = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
                break;
            case QuestButton.BButtonRight:
                pressed = OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch);
                break;
            case QuestButton.XButtonLeft:
                pressed = OVRInput.GetDown(OVRInput.Button.Three, OVRInput.Controller.LTouch);
                break;
            case QuestButton.YButtonLeft:
                pressed = OVRInput.GetDown(OVRInput.Button.Four, OVRInput.Controller.LTouch);
                break;
            case QuestButton.TriggerRight:
                pressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
                break;
            case QuestButton.TriggerLeft:
                pressed = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
                break;
            case QuestButton.GripRight:
                pressed = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);
                break;
            case QuestButton.GripLeft:
                pressed = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);
                break;
            case QuestButton.ThumbstickPressRight:
                pressed = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch);
                break;
            case QuestButton.ThumbstickPressLeft:
                pressed = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch);
                break;
            default:
                pressed = false;
                break;
        }

        if (pressed)
        {
            Debug.Log($"[{gameObject.name}] ボタンが押されました: {triggerButton}");
        }

        return pressed;
    }

    /// <summary>
    /// 傾き角度チェック（検知のみ）
    /// </summary>
    protected virtual bool CheckTiltAngle()
    {
        if (currentContainer == null)
        {
            Debug.Log($"[{gameObject.name}] CheckTiltAngle: currentContainerがnullです");
            return false;
        }

        // 傾き判定
        float angleX = Mathf.Abs(currentContainer.transform.eulerAngles.x);
        float angleZ = Mathf.Abs(currentContainer.transform.eulerAngles.z);

        // 180度を超える場合は補正
        if (angleX > 180f) angleX = 360f - angleX;
        if (angleZ > 180f) angleZ = 360f - angleZ;

        float maxAngle = Mathf.Max(angleX, angleZ);
        bool isTilted = maxAngle > tiltAngleThreshold;

        if (isTilted)
        {
            Debug.Log($"[{gameObject.name}] CheckTiltAngle: 傾いています。angleX={angleX:F1}度, angleZ={angleZ:F1}度, maxAngle={maxAngle:F1}度, threshold={tiltAngleThreshold}度");
        }

        return isTilted;
    }

    // === サブクラスで実装するメソッド ===

    /// <summary>
    /// 器具が範囲内に入った時の処理
    /// </summary>
    protected virtual void OnContainerEntered(WaterVessel container)
    {
        // サブクラスで実装
    }

    /// <summary>
    /// 器具が範囲外に出た時の処理
    /// </summary>
    protected virtual void OnContainerExited(WaterVessel container)
    {
        // サブクラスで実装
    }

    /// <summary>
    /// タスク実行（サブクラスで実装）
    /// </summary>
    protected abstract void ExecuteTask();
}

