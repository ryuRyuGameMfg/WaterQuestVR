# VR Water Resource Game - 開発ガイド

**VR Water Resource Serious Game (MVP) 統合ドキュメント**

このドキュメントは、ゲーム仕様・システム設計・実装計画を統合した開発ガイドです。

---

## 目次

- [1. プロジェクト概要](#1-プロジェクト概要)
- [2. ゲーム仕様](#2-ゲーム仕様)
- [3. システム設計](#3-システム設計)
- [4. 実装計画](#4-実装計画)

---

# 1. プロジェクト概要

## プロジェクト名
**VR Water Resource Serious Game (MVP)**

## 目的
Unity 6 + Meta Quest で動作する水資源教育用VRゲームの最小構成版（MVP）を開発する。

## 基本コンセプト

本プロジェクトは、**Meta Quest** 上で動作する「水資源問題の理解促進」を目的とした **VRシリアスゲーム** です。

プレイヤーは「井戸から水を汲み、生活に使用する」体験を通して、水資源の有限性や意思決定の影響を学習します。

Unity 6 を使用し、教育・研究用途に最適化された最小構成（MVP）として開発します。
既存のUnityアセットを活用することで、開発コストを抑えながらリアルな環境体験を実現します。

---

# 2. ゲーム仕様

## 環境構成

### シーン構成
- **舞台**: 農村の屋外空間（井戸・畑・家屋外周）
- **登場オブジェクト**: 井戸、バケツ、コップ、畑、建物、地面など
- **3Dモデル**: 無料アセットを中心に構築（有料アセット使用時は事前相談）
- **光源・環境効果**: Built-in Render Pipeline 軽量設定

### 利用可能な既存アセット
- **Viking Village**: 農村環境、建物、井戸、フェンス等
- **VillagePack**: 家屋、小物
- **Mugs, Bowls and Plates**: 食器類（コップ代用可能）

---

## 器具容量の定義

| 器具名 | 最大水量 | 備考 |
|:--|:--:|:--|
| バケツ | 80 | 農業・水汲み・廃棄に使用 |
| コップ | 10 | 飲用に使用 |

---

## 各タスク別パラメータ設定（仮定値）

| 行動 | 水量変化 | 水質変化 | 体力変化 | 使用器具 | 備考 |
|:--|:--:|:--:|:--:|:--|:--|
| 井戸から水を汲む | ＋80（満水） | ＋10（清潔度上昇） | −10 | バケツ or コップ | 清浄な地下水を取得 |
| 水を捨てる | −20（または残量すべて） | −10（環境悪化） | −2 | バケツ or コップ | 廃棄による周囲の汚染を反映 |
| 農業を行う（畑に水をまく） | −50 | −5（使用による水質低下） | −15 | バケツ | 農業用水として使用、効率に反映 |
| 飲む | −10 | ±0（直前の水質を参照） | ＋10（※水質80以上時）／−10（80未満時） | コップ | 清潔な水で回復、不衛生なら減少 |

---

## パラメータ管理

ゲームでは以下の3つの主要パラメータを管理します：

| パラメータ | 型 | 範囲 | 説明 |
|:--|:--:|:--:|:--|
| 水量（Water Volume） | **float** | 0.0〜100.0 | 現在保持している水の量。行動に応じて増減。 |
| 水質（Water Quality） | **float** | 0.0〜100.0 | 水の清潔度。汚染行動で低下、浄水で上昇。 |
| 体力（Stamina） | **float** | 0.0〜100.0 | プレイヤーの体力。行動で減少、飲用で回復。 |

**データ型**: すべて `float` 型（小数点以下も扱う、柔軟性のため）

**更新タイミング**: リアルタイムではなく、「タスク実行時（イベント発生時）」にのみ反映されます。

---

## パラメータ閾値と定義

| 項目 | 定義 | 効果 |
|:--|:--|:--|
| 水質80以上 | 安全域（飲用可能） | 飲用時に体力回復 |
| 水質79以下 | 汚染域（飲用リスク） | 飲用時に衛生リスク・体力減少 |

---

## スコアリング仕様

体験終了時に、以下の2つの指標を算出します。

### 衛生（Hygiene）

**計算式:**
```
衛生 = 1 - (不適切用途の使用量 ÷ 全使用量)
```

- **不適切用途の使用量**: 水質が80未満の状態で飲用に使用した水量の合計
- **全使用量**: 飲用・農業・廃棄を含めた水使用総量

最終的に0〜1の値を0〜100スコアに変換し表示します。

### 効率（Efficiency）

**計算式:**
```
効率 = (生活で使用した水量 ÷ 汲み上げた水量)
```

- **生活で使用した水量**: 飲用および農業に実際に使用した水量（廃棄を除く）
- **汲み上げた水量**: 井戸から取得した総水量

効率スコアも0〜1を0〜100に変換して表示します。

---

## 操作仕様

- **対応デバイス**: Meta Quest
- **VRフレームワーク**: Meta XR SDK（OVRCameraRig, OVRHand, OVRInput）
- **操作方式**: ハンドトラッキング + コントローラ操作（手で掴む／注ぐ動作）
- **行動ごとの効果**: 体力・水質・水量がイベントベースで変動
- **入力方式**: Meta XR SDK ネイティブAPI（既存のCarriableシステムを拡張）

**重要**: OpenXR はメインではなく、Meta XR SDK のネイティブ機能を優先使用

---

## UI仕様

- **表示形式**: HUD固定表示（視界上部に数値表示）
- **表示項目**: 水量・水質・体力（各0〜100整数値）
- **表示更新**: 行動イベント発生時のみ更新
- **拡張案**: 手首UI（余裕があれば追加）

---

## 技術構成

| 項目 | 内容 |
|:--|:--|
| ゲームエンジン | Unity 6 (6000.2.7f2) |
| ビルドターゲット | Meta Quest（Androidビルド） |
| レンダリング | Built-in Render Pipeline（軽量設定） |
| 物理挙動 | Obi Fluid（簡易流体表現） |
| **VRフレームワーク** | **Meta XR SDK v78.0.0（ネイティブAPI使用）** |
| XR Plugin | Meta XR Plugin（OpenXR はサブ扱い） |
| インタラクション | OVRHand + Carriableシステム拡張 |

**備考**: Meta Quest上では高度なCompute流体挙動は非対応のため、簡易表現に留めます。

---

## 留意事項

- Meta Quest 実機上での動作を前提とした最適化を実施
- 流体表現・水質変化は教育用デモとして簡易実装とする
- 各数値は仮値（MVP段階）であり、実測またはユーザーテスト後に調整可能
- 既存のVikingVillageアセットを最大限活用しコスト削減

---

## 付記（今後の拡張候補）

- 雨や濁流による自然水質変動イベント
- 地域別水資源量データとの連動（教育研究向け）
- 行動履歴のCSV出力機能（学習評価用）
- マルチプレイヤー対応（複数人での水資源管理体験）
- アクセシビリティ機能（字幕、音声ガイド）

---

# 3. システム設計

## 設計方針

### 超シンプル設計（数値のみの制御）

#### 水量管理の2値化
- **プログラム内部**: 満タン(`bool isFull`) / 空 の2値のみ
- **満タンの定義**: コップ10、バケツ80（アイテムごとに異なる、インスペクター設定可能）
- **見た目（Obi）**: 連続的に表示（エフェクトで調整）
- **水の消費**: 傾けたかどうかで判定

#### 傾き判定で制御
- **傾けた** → 水が空になる
- **傾き閾値**: インスペクターで設定可能（デフォルト45度）
- **水捨て**: 何もない場所で傾ければ自動的に廃棄

#### タスクごとの水量変化（インスペクター設定可能）
- 井戸: +80（バケツ）、+10（コップ）
- 畑: -50
- 飲用: -10
- これらの値はすべてインスペクターで変更可能

#### 完全削除する機能
- ❌ エフェクト・アニメーション（すべてのタスクで不要）
- ❌ 音声（すべてのタスクで不要）
- ❌ パーティクル（すべてのタスクで不要）
- ❌ 水の色変化
- ❌ WaterDisposalZone（傾ければ自動廃棄）

#### 実装する機能（中核のみ）
- ✅ パラメータ管理（`float` 型で細かく記録）
- ✅ 水器具システム（2値の状態管理 + インスペクター設定）
- ✅ 角度ベースの傾き判定
- ✅ インタラクションオブジェクト（井戸・畑・飲用のみ）
- ✅ スコア計算（衛生・効率）
- ✅ HUD表示（数値のみ）
- ✅ 統計記録（細かい数値で記録）

#### 別途実装する機能
- VRインタラクション（既存 Carriable.cs を利用）
- 見た目のエフェクト（Obiのエフェクトで調整）

---

## システムアーキテクチャ

### 超シンプル2層構造

```
┌─────────────────────────────────────────┐
│    Manager Layer                        │
│  - GameManager（タスク管理・統計）       │
│    └ GameData（データ保持）              │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│    Object Layer                         │
│  - WaterContainer (Bucket, Cup)         │
│  - Task Objects (WaterTap, Field, etc)  │
│  - UI (HUD, Result)                     │
└─────────────────────────────────────────┘
```

**設計方針**:
- **Manager Layer**: GameManager 1つで全体管理（タスクカウント、パラメータ、統計）
- **Object Layer**: 個別オブジェクト（器具、タスク場所、UI）
- 小規模開発のため、クラスを最小限に抑える
- **自動終了**: タスク数が設定値に達したら自動的にリザルト表示（ボタン不要）

---

## クラス設計

### 1. Manager Layer（ゲーム管理）

#### ActionHistory.cs
**役割**: ゲームセッション全体の行動履歴

```csharp
[System.Serializable]
public class ActionHistory
{
    // 水量トラッキング
    public float WaterDrawn;           // 汲んだ水量
    public float WaterUsedForFarming;  // 畑に使った水量
    public float WaterUsedForDrinking; // 飲んだ水量
    public float WaterUsedForWashing;  // 洗濯に使った水量（将来用）
    public float WaterWasted;          // 捨てた水量
    public float WaterPolluted;        // 汚染された水量

    // 行動回数
    public int DrawCount;
    public int FarmCount;
    public int DrinkCount;
    public int WashCount;
    public int WasteCount;

    // 総タスク数（自動計算）
    public int TotalTasksCompleted => DrawCount + FarmCount + DrinkCount + WasteCount;
}
```

**備考**: 各行動が実行されるたびにカウントが増加し、`TotalTasksCompleted` が設定値に達したら自動的にリザルトを表示します。

#### GameData.cs
**役割**: ゲーム全体のデータ保持

```csharp
[System.Serializable]
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
```

#### GameManager.cs
**役割**: ゲーム全体の制御（シングルトン）

```csharp
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

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordFarming(float amount)
    {
        Data.WaterVolume -= amount;
        Data.WaterQuality -= 5f; // 水質低下
        Data.History.WaterUsedForFarming += amount;
        Data.History.FarmCount++;

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    public void RecordDrinking(float amount, float quality)
    {
        Data.WaterVolume -= amount;
        bool isSafe = quality >= SAFE_QUALITY_THRESHOLD;

        // 体力変化
        Data.Stamina += isSafe ? 10f : -10f;
        Data.Stamina = Mathf.Clamp(Data.Stamina, 0f, 100f);

        Data.History.WaterUsedForDrinking += amount;
        Data.History.DrinkCount++;

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

        OnTaskCompleted?.Invoke();
        CheckGameCompletion();  // タスク完了後に自動チェック
    }

    // === スコア計算 ===

    public float CalculateHygiene()
    {
        float totalUsed = 0f;
        float unsafeUsed = 0f;

        totalUsed = Data.History.WaterUsedForDrinking + Data.History.WaterUsedForFarming;
        // 不適切飲用量の計算は省略（簡易版）

        if (totalUsed == 0) return 100f;
        return (1f - (unsafeUsed / totalUsed)) * 100f;
    }

    public float CalculateEfficiency()
    {
        float totalDrawn = 0f;
        float totalUsed = 0f;

        totalDrawn = Data.History.WaterDrawn;
        totalUsed = Data.History.WaterUsedForDrinking + Data.History.WaterUsedForFarming;

        if (totalDrawn == 0) return 0f;
        return (totalUsed / totalDrawn) * 100f;
    }

    private void ShowResults()
    {
        // リザルトUI表示（ResultUI.csで実装）
        Debug.Log($"ゲーム終了 - 衛生: {CalculateHygiene():F1}, 効率: {CalculateEfficiency():F1}");
    }
}
```

**使用例:**

```csharp
// ゲーム開始時（自動）
// VRスポーン時に Start() で自動的に Data.Reset() が実行される

// 各行動の記録（タスクとして自動カウント）
GameManager.Instance.RecordDrawWater(80f, 100f);  // 水道口で水を汲む（タスク+1）
GameManager.Instance.RecordFarming(50f);           // 畑に水をまく（タスク+1）
GameManager.Instance.RecordDrinking(10f, 95f);     // 水を飲む（タスク+1）
GameManager.Instance.RecordWaste(20f);             // 水を捨てる（タスク+1）

// タスク数が MaxTasks に達すると自動的にリザルト表示（ボタン不要）

// データアクセス
int taskCount = GameManager.Instance.Data.History.TotalTasksCompleted;
float stamina = GameManager.Instance.Data.Stamina;
ActionHistory history = GameManager.Instance.Data.History;

// 統計確認
Debug.Log($"総タスク数: {history.TotalTasksCompleted}");
Debug.Log($"水汲み{history.DrawCount}回, 畑{history.FarmCount}回, 飲用{history.DrinkCount}回");
```

---

### 2. Object Layer（個別オブジェクト）

水の状態は各WaterContainerで `bool isFull` として管理します。

#### WaterContainer.cs (抽象基底クラス)
**役割**: バケツ・コップの共通機能

```csharp
public abstract class WaterContainer : MonoBehaviour
{
    // 満タン時の容量（インスペクターで設定）
    [SerializeField] protected float maxCapacity = 80f;
    public float MaxCapacity => maxCapacity;

    // 現在の水質（インスペクターで設定可能、デフォルト100）
    [SerializeField] protected float waterQuality = 100f;
    public float WaterQuality => waterQuality;

    // 水の状態（満タン or 空）
    protected bool isFull = false;
    public bool IsFull => isFull;

    // 注ぐ判定の角度閾値（インスペクターで設定）
    [SerializeField] protected float pourAngleThreshold = 45f;

    // 前フレームの傾き状態
    private bool wasPouringLastFrame = false;

    // イベント
    public event Action<float> OnWaterFilled;    // 満タンになった時
    public event Action<float> OnWaterPoured;    // 空になった時

    // メソッド
    public virtual void FillWater(float quality)
    {
        isFull = true;
        waterQuality = quality;
        OnWaterFilled?.Invoke(maxCapacity);
        UpdateVisuals();
    }

    public virtual void EmptyWater()
    {
        if (!isFull) return; // すでに空なら何もしない

        isFull = false;
        OnWaterPoured?.Invoke(maxCapacity);
        UpdateVisuals();
    }

    // 注ぐ判定（角度ベース）
    public virtual bool IsPouringAngle()
    {
        float angleX = Mathf.Abs(transform.eulerAngles.x);
        float angleZ = Mathf.Abs(transform.eulerAngles.z);

        // 0-180度の範囲に正規化
        if (angleX > 180f) angleX = 360f - angleX;
        if (angleZ > 180f) angleZ = 360f - angleZ;

        return angleX > pourAngleThreshold || angleZ > pourAngleThreshold;
    }

    // 毎フレーム傾き監視
    protected virtual void Update()
    {
        bool isPouringNow = IsPouringAngle();

        // 傾けた瞬間を検知
        if (isPouringNow && !wasPouringLastFrame && isFull)
        {
            OnTilted();
        }

        wasPouringLastFrame = isPouringNow;
    }

    // 傾けた時の処理（サブクラスでオーバーライド可能）
    protected virtual void OnTilted()
    {
        // デフォルト: 何もしない（タスク場所で個別に処理）
    }

    // 視覚的フィードバック（見た目のみ、Obiで調整）
    protected abstract void UpdateVisuals();
}
```

**設計のポイント:**

1. **2値管理**: `bool isFull` で満タン/空を管理
2. **インスペクター設定可能**: 容量、水質、傾き閾値
3. **傾けた瞬間を検知**: `OnTilted()` でタスク場所に応じた処理
4. **シンプルなAPI**: `FillWater()` と `EmptyWater()` のみ

#### Bucket.cs
**役割**: バケツ（デフォルト容量80）

```csharp
public class Bucket : WaterContainer
{
    [SerializeField] private GameObject waterVisual; // 水の視覚表現（Obiエフェクト）

    protected override void UpdateVisuals()
    {
        // 満タン/空で見た目を切り替え
        // （実際の見た目はObiエフェクトで連続的に調整）
        waterVisual.SetActive(isFull);
    }
}
```

**インスペクター設定例:**
- maxCapacity: 80
- pourAngleThreshold: 45

#### Cup.cs
**役割**: コップ（デフォルト容量10）

```csharp
public class Cup : WaterContainer
{
    [SerializeField] private GameObject waterVisual; // 水の視覚表現（Obiエフェクト）

    protected override void UpdateVisuals()
    {
        // 満タン/空で見た目を切り替え
        waterVisual.SetActive(isFull);
    }
}
```

**インスペクター設定例:**
- maxCapacity: 10
- pourAngleThreshold: 45

---

### 3. インタラクション（Interactions）

各タスク場所（WaterTap, Field, DrinkingPoint）は独自にTrigger判定と傾き検知を実装します。

#### WaterTap.cs
**役割**: 水道口（蛇口）- 水を汲む

```csharp
public class WaterTap : MonoBehaviour
{
    // インスペクターで設定可能
    [SerializeField] private GameObject obiWaterEffect;      // Obiの水エフェクト
    [SerializeField] private float waterQuality = 100f;      // 汲める水の水質
    [SerializeField] private float staminaCost = 10f;        // 体力コスト

    // トリガー設定（インスペクターで選択）
    [SerializeField] private bool useButtonTrigger = true;   // ボタン押下で水を出す
    [SerializeField] private bool useCollisionTrigger = false; // 器具接触で水を出す

    private WaterContainer currentContainer = null;
    private bool isWaterFlowing = false;

    private void OnTriggerEnter(Collider other)
    {
        // 器具が水道口付近に入った
        WaterContainer container = other.GetComponent<WaterContainer>();
        if (container != null && !container.IsFull)
        {
            currentContainer = container;

            // 衝突トリガーモードの場合、自動的に水を出す
            if (useCollisionTrigger)
            {
                StartWaterFlow();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WaterContainer container = other.GetComponent<WaterContainer>();
        if (container == currentContainer)
        {
            StopWaterFlow();
            currentContainer = null;
        }
    }

    private void Update()
    {
        // ボタントリガーモード: OVRInput でボタン検出
        if (useButtonTrigger && currentContainer != null && !isWaterFlowing)
        {
            // 例: Aボタン or ピンチジェスチャー
            if (OVRInput.GetDown(OVRInput.Button.One)) // Aボタン
            {
                StartWaterFlow();
            }
        }
    }

    private void StartWaterFlow()
    {
        if (currentContainer == null || currentContainer.IsFull) return;

        // Obiエフェクトを表示（水が出る）
        if (obiWaterEffect != null)
        {
            obiWaterEffect.SetActive(true);
        }

        isWaterFlowing = true;

        // 器具に水を満タンにする
        currentContainer.FillWater(waterQuality);

        // GameManagerに記録
        float amount = currentContainer.MaxCapacity;
        GameManager.Instance.RecordDrawWater(amount, waterQuality);
        GameManager.Instance.Data.Stamina -= staminaCost;

        // 一定時間後に水を止める
        Invoke(nameof(StopWaterFlow), 2f);
    }

    private void StopWaterFlow()
    {
        // Obiエフェクトを非表示（水が止まる）
        if (obiWaterEffect != null)
        {
            obiWaterEffect.SetActive(false);
        }

        isWaterFlowing = false;
    }
}
```

**インスペクター設定:**
- obiWaterEffect: Obiの水エフェクトGameObject
- waterQuality: 100（清浄な水）
- staminaCost: 10
- useButtonTrigger: true（ボタン押下で水を出す）
- useCollisionTrigger: false（器具接触で自動的に水を出す）

**動作:**
1. 空の器具を水道口（蛇口）付近に持っていく
2. トリガー方法（インスペクターで選択）:
   - **ボタンモード**: Aボタンを押すと水が出る
   - **衝突モード**: 器具が当たると自動的に水が出る
3. Obiエフェクトが表示され、水が流れる演出
4. 器具が満タンになり、2秒後に水が止まる（エフェクト非表示）

#### Field.cs
**役割**: 畑（水をまく）

```csharp
public class Field : MonoBehaviour
{
    // インスペクターで設定可能
    [SerializeField] private float waterConsumption = 50f;   // 消費する水量
    [SerializeField] private float qualityDecrease = 5f;     // 水質低下
    [SerializeField] private float staminaCost = 15f;        // 体力コスト

    private WaterContainer currentContainer = null;

    private void OnTriggerEnter(Collider other)
    {
        // バケツが範囲内に入った
        WaterContainer container = other.GetComponent<WaterContainer>();
        if (container != null && container is Bucket)
        {
            currentContainer = container;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // バケツが範囲外に出た
        WaterContainer container = other.GetComponent<WaterContainer>();
        if (container == currentContainer)
        {
            currentContainer = null;
        }
    }

    private void Update()
    {
        // 範囲内でバケツを傾けたら水やり実行
        if (currentContainer != null &&
            currentContainer.IsFull &&
            currentContainer.IsPouringAngle())
        {
            PourWater(currentContainer);
        }
    }

    private void PourWater(WaterContainer container)
    {
        // 水を空にする
        container.EmptyWater();

        // GameManagerに記録
        GameManager.Instance.RecordFarming(waterConsumption);
        GameManager.Instance.Data.Stamina -= staminaCost;
    }
}
```

**インスペクター設定例:**
- waterConsumption: 50
- qualityDecrease: 5
- staminaCost: 15

**動作:**
1. バケツを畑の範囲内に持っていく
2. 傾ける → 水やり実行、バケツが空になる

#### DrinkingPoint.cs
**役割**: 飲用ポイント

```csharp
public class DrinkingPoint : MonoBehaviour
{
    // インスペクターで設定可能
    [SerializeField] private float waterConsumption = 10f;        // 消費する水量
    [SerializeField] private float safeStaminaGain = 10f;         // 安全な水の体力回復
    [SerializeField] private float unsafeStaminaLoss = 10f;       // 汚染水の体力減少
    [SerializeField] private float safeQualityThreshold = 80f;    // 安全閾値

    private WaterContainer currentContainer = null;

    private void OnTriggerEnter(Collider other)
    {
        // コップが範囲内に入った
        WaterContainer container = other.GetComponent<WaterContainer>();
        if (container != null && container is Cup)
        {
            currentContainer = container;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // コップが範囲外に出た
        WaterContainer container = other.GetComponent<WaterContainer>();
        if (container == currentContainer)
        {
            currentContainer = null;
        }
    }

    private void Update()
    {
        // 範囲内でコップを傾けたら飲用実行
        if (currentContainer != null &&
            currentContainer.IsFull &&
            currentContainer.IsPouringAngle())
        {
            Drink(currentContainer);
        }
    }

    private void Drink(WaterContainer container)
    {
        // 水質チェック
        float quality = container.WaterQuality;

        // 水を空にする
        container.EmptyWater();

        // GameManagerに記録（体力変化も含む）
        GameManager.Instance.RecordDrinking(waterConsumption, quality);
    }
}
```

**インスペクター設定例:**
- waterConsumption: 10
- safeStaminaGain: 10
- unsafeStaminaLoss: 10
- safeQualityThreshold: 80

**動作:**
1. コップを口元（飲用ポイント）に持っていく
2. 傾ける → 飲用実行、コップが空になる
3. 水質80以上なら体力+10、未満なら体力-10

どこでも傾ければ自動的に水を捨てたことになります。

**水の廃棄処理:**

WaterContainerの `OnTilted()` で自動的に処理します：

```csharp
protected override void OnTilted()
{
    // タスク場所にいない場合は自動廃棄
    if (!IsNearTaskLocation())
    {
        AutoDispose();
    }
}

private void AutoDispose()
{
    EmptyWater();

    // GameManagerに記録（環境汚染・体力コスト含む）
    GameManager.Instance.RecordWaste(maxCapacity);
    GameManager.Instance.Data.Stamina -= 2f;
}
```

---

### 4. UI システム

#### ResultUI.cs
**役割**: リザルト画面表示

```csharp
using TMPro;
using UnityEngine;

public class ResultUI : MonoBehaviour
{
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private TextMeshProUGUI actionDetailsText;
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        GameManager.Instance.OnGameEnded += ShowResults;
        resultPanel.SetActive(false);
    }

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
```

#### HUDManager.cs
**役割**: HUD全体の制御

```csharp
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Transform playerHead; // VRカメラ（OVRCameraRig）

    [SerializeField] private TextMeshProUGUI taskProgressText;
    [SerializeField] private TextMeshProUGUI waterVolumeText;
    [SerializeField] private TextMeshProUGUI waterQualityText;
    [SerializeField] private TextMeshProUGUI staminaText;

    private void Update()
    {
        // GameDataから直接読み込み
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
}
```

**備考**: ボタン不要（タスク数が自動的にカウントされ、MaxTasks達成で自動終了）

---

## データフロー（シンプル版）

### 水道口から水を汲む場合

```
┌──────────────────┐
│ Player Input     │ 器具を水道口付近に持っていく + ボタン押下
└──────┬───────────┘
       ↓
┌──────────────────┐
│ WaterTap         │ Obiエフェクト表示 → 器具に水を追加（容量分）
└──────┬───────────┘
       ↓
┌─────────────────────────────────┐
│ ParameterManager                │
│ - 水量 +80                       │
│ - 水質 +10                       │
│ - 体力 -10                       │
└──────┬──────────────────────────┘
       ↓
┌─────────────────────────────────┐
│ StatisticsTracker               │
│ - 汲み上げた水量を記録           │
└──────┬──────────────────────────┘
       ↓
┌─────────────────────────────────┐
│ HUD 更新                         │
│ - パラメータ表示を更新           │
└─────────────────────────────────┘
```

**注**: エフェクト・アニメーションは実装しない。パラメータ更新とHUD表示のみ。

---

## ファイル構成

```
Assets/
├── Scenes/
│   └── MainGameScene.unity         # メインゲームシーン
│
├── Scripts/
│   ├── GameManagement/
│   │   ├── GameManager.cs         # ゲーム全体制御（タスク管理・統計）
│   │   ├── GameData.cs            # データ保持
│   │   └── ActionHistory.cs       # ゲームセッション全体の履歴
│   │
│   ├── WaterSystem/
│   │   ├── WaterContainer.cs      # 抽象基底クラス（2値管理）
│   │   ├── Bucket.cs              # デフォルト容量80
│   │   └── Cup.cs                 # デフォルト容量10
│   │
│   ├── Interactions/
│   │   ├── WaterTap.cs            # 水道口（水を汲む）
│   │   ├── Field.cs               # 水をまく
│   │   └── DrinkingPoint.cs       # 飲用
│   │
│   └── UI/
│       ├── HUDManager.cs          # HUD制御（タスク進行表示）
│       └── ResultUI.cs            # リザルト表示（自動表示）
│
├── Prefabs/
│   ├── Bucket.prefab              # Obiエフェクト付き
│   ├── Cup.prefab                 # Obiエフェクト付き
│   ├── WaterTap.prefab            # 水道口（Obiエフェクト + Trigger）
│   ├── Field.prefab               # Trigger Collider付き
│   ├── DrinkingPoint.prefab       # Trigger Collider付き
│   └── HUD.prefab
│
└── Materials/
    └── UI_Materials/ (HUD用)
```

**主要クラス:**
- `GameManager.cs` - ゲーム全体制御（タスク管理・統計・スコア計算）
- `GameData.cs` - データ保持（パラメータ・履歴）
- `ActionHistory.cs` - ゲームセッション全体の行動履歴
- `WaterContainer.cs` - 器具の抽象基底クラス（2値管理）
- `Bucket.cs` / `Cup.cs` - 具体的な器具
- `WaterTap.cs` - 水道口（水を汲む）
- `Field.cs` - 畑（水をまく）
- `DrinkingPoint.cs` - 飲用場所
- `HUDManager.cs` - タスク進行表示
- `ResultUI.cs` - リザルト画面（自動表示）

---

## パフォーマンス最適化

### Quest 向け最適化戦略

1. **描画負荷削減**
   - Obi Fluidのパーティクル数: 最大500個
   - テクスチャ解像度: 1024x1024以下
   - ポリゴン数: シーン全体で50,000以下

2. **物理演算最適化**
   - Fixed Timestep: 0.02 (50Hz)
   - Collision Matrix設定（不要な衝突判定を無効化）

3. **スクリプト最適化**
   - Update()の使用を最小限に
   - イベント駆動型（パラメータ変更時のみ処理）
   - オブジェクトプール（パーティクル再利用）

---

---

**作成日**: 2025-10-25
**最終更新**: 2025-10-25
**バージョン**: MVP 1.0

**タスク管理**: 詳細なタスクは `docs/TASKS.md` を参照してください。
