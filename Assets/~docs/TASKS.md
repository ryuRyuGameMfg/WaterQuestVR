# 開発タスク一覧

このファイルは、VR Water Resource Serious Game (MVP) の開発タスクをチェックリスト形式で管理します。

**最終更新**: 2025-10-25

---

## Phase 1: 環境構築・基盤整備

### 1.1 シーン構築

- [ ] 新規シーン作成
  - [ ] `Assets/Scenes/MainGameScene.unity` 作成
  - [ ] VR カメラリグ配置（OVRCameraRig）
  - [ ] ライティング設定（Directional Light）
  - [ ] Skybox設定

- [ ] Viking Village アセットから環境配置
  - [ ] 井戸オブジェクト配置（`Assets/Viking Village/Models/Well/`）
  - [ ] 家屋外観配置
  - [ ] 地面・Terrain設定
  - [ ] フェンス・小物配置
  - [ ] 畑エリア作成（Plane + マテリアル）

- [ ] 環境最適化
  - [ ] Occlusion Culling設定
  - [ ] LOD設定（必要に応じて）
  - [ ] ライトマップ焼き込み（Static Lighting）

### 1.2 VR インタラクション基盤

- [ ] Carriable システムの理解
  - [ ] 既存 `Carriable.cs` のコード確認
  - [ ] OVRHand との連携確認
  - [ ] ピンチジェスチャーの動作テスト

- [ ] 水器具モデルの準備
  - [ ] バケツ3Dモデル（Asset Store または自作）
  - [ ] コップ3Dモデル（`Mugs, Bowls and Plates` から流用）
  - [ ] Collider設定（Mesh Collider or Box Collider）

- [ ] Carriable拡張
  - [ ] バケツに Carriable コンポーネント追加
  - [ ] コップに Carriable コンポーネント追加
  - [ ] 掴み・離しの動作確認（Quest実機）

---

## Phase 2: コアメカニクス実装

### 2.1 パラメータ管理システム

- [ ] GameManager.cs 実装
  - [ ] シングルトンパターン実装
  - [ ] GameState enum 定義
  - [ ] StartGame(), EndGame(), PauseGame() メソッド
  - [ ] 他マネージャーへの参照保持

- [ ] ParameterManager.cs 実装
  - [ ] WaterVolume, WaterQuality, Stamina プロパティ
  - [ ] Modify系メソッド（ModifyWaterVolume等）
  - [ ] Clamp処理（0-100範囲制限）
  - [ ] UnityEvent または C# Event 実装
  - [ ] IsWaterSafe() メソッド（閾値80判定）

- [ ] ScoreCalculator.cs 実装
  - [ ] CalculateHygiene() メソッド
  - [ ] CalculateEfficiency() メソッド
  - [ ] StatisticsTracker との連携

- [ ] StatisticsTracker.cs 実装
  - [ ] 統計変数定義（TotalWaterDrawn等）
  - [ ] Record系メソッド（RecordWaterDrawn等）
  - [ ] Reset() メソッド
  - [ ] （オプション）ExportToCSV() メソッド

### 2.2 水器具システム

- [ ] WaterProperties.cs 実装
  - [ ] Volume, Quality フィールド
  - [ ] コンストラクタ（初期値設定）
  - [ ] GetWaterColor() メソッド（品質→色変換）

- [ ] WaterContainer.cs 実装（抽象クラス）
  - [ ] MaxCapacity 抽象プロパティ
  - [ ] CurrentWater プロパティ
  - [ ] ReceiveWater() メソッド
  - [ ] PourWater() メソッド
  - [ ] EmptyContainer() メソッド
  - [ ] UpdateVisuals() 抽象メソッド
  - [ ] OnWaterReceived, OnWaterPoured イベント

- [ ] Bucket.cs 実装
  - [ ] MaxCapacity = 80 設定
  - [ ] UpdateVisuals() 実装（水の高さ・色変更）
  - [ ] Prefab作成（`Assets/Prefabs/Bucket.prefab`）

- [ ] Cup.cs 実装
  - [ ] MaxCapacity = 10 設定
  - [ ] UpdateVisuals() 実装
  - [ ] Prefab作成（`Assets/Prefabs/Cup.prefab`）

### 2.3 インタラクション可能オブジェクト

- [ ] IWaterInteractable.cs 実装
  - [ ] CanInteract() メソッド定義
  - [ ] Interact() メソッド定義
  - [ ] GetInteractionPrompt() メソッド定義

- [ ] Well.cs 実装
  - [ ] IWaterInteractable 実装
  - [ ] Interact() ロジック（水量+80, 水質+10, 体力-10）
  - [ ] StatisticsTracker への記録
  - [ ] 音声・エフェクト再生
  - [ ] Prefab作成（`Assets/Prefabs/Well.prefab`）

- [ ] Field.cs 実装
  - [ ] IWaterInteractable 実装
  - [ ] Interact() ロジック（水量-50, 水質-5, 体力-15）
  - [ ] バケツのみ使用可能チェック
  - [ ] 水やりパーティクルエフェクト
  - [ ] Prefab作成（`Assets/Prefabs/Field.prefab`）

- [ ] DrinkingPoint.cs 実装
  - [ ] IWaterInteractable 実装
  - [ ] Interact() ロジック（水量-10, 体力±10）
  - [ ] 水質閾値判定（80以上/未満）
  - [ ] フィードバック表示（安全/危険メッセージ）
  - [ ] シーン配置（プレイヤー周辺のトリガーゾーン）

- [ ] WaterDisposalZone.cs 実装
  - [ ] IWaterInteractable 実装
  - [ ] Interact() ロジック（水量-20, 水質-10, 体力-2）
  - [ ] 廃棄エフェクト
  - [ ] シーン配置

### 2.4 インタラクショントリガー

- [ ] InteractionTrigger.cs 実装（汎用）
  - [ ] OnTriggerEnter/Stay/Exit でインタラクション検知
  - [ ] WaterContainer の保持確認
  - [ ] IWaterInteractable の Interact() 呼び出し
  - [ ] UIプロンプト表示（「Aボタンで水を汲む」等）

- [ ] 各オブジェクトにトリガー設定
  - [ ] Well にトリガーCollider追加
  - [ ] Field にトリガーCollider追加
  - [ ] DrinkingPoint にトリガーCollider追加
  - [ ] WaterDisposalZone にトリガーCollider追加

---

## Phase 3: 流体表現（簡易版）

### 3.1 Obi Fluid 設定

- [ ] Obi Fluid の動作確認
  - [ ] `Assets/Obi/Samples/` で既存サンプル確認
  - [ ] Built-in RP シェーダーの動作確認

- [ ] バケツ用 Obi Emitter 設定
  - [ ] ObiEmitter コンポーネント追加
  - [ ] ObiSolver 設定（パーティクル数制限: 最大500）
  - [ ] パーティクルマテリアル設定（青色）

- [ ] 注ぐ動作のパーティクル
  - [ ] バケツを傾けたときのパーティクル放出
  - [ ] 重力方向への自然な流れ
  - [ ] パフォーマンステスト（Quest実機）

### 3.2 視覚的フィードバック

- [ ] 水量に応じた視覚表現
  - [ ] 器具内の水面高さ（Scale調整）
  - [ ] 満水時・空時の見た目変化

- [ ] 水質に応じた色変化
  - [ ] WaterProperties.GetWaterColor() の適用
  - [ ] 水質80以上: 青
  - [ ] 水質50-79: 緑
  - [ ] 水質50未満: 茶色

- [ ] エフェクト追加
  - [ ] 水汲み時のパーティクル
  - [ ] 水やり時のパーティクル
  - [ ] 廃棄時のパーティクル

---

## Phase 4: UI実装

### 4.1 HUD表示

- [ ] Canvas作成（World Space）
  - [ ] Canvas コンポーネント設定
  - [ ] Render Mode: World Space
  - [ ] プレイヤーカメラ追従設定

- [ ] HUDManager.cs 実装
  - [ ] Canvas の位置・回転更新（LateUpdate）
  - [ ] ParameterManager のイベント購読
  - [ ] ParameterDisplay への通知

- [ ] ParameterDisplay.cs 実装
  - [ ] TextMeshProUGUI 配置（水量・水質・体力）
  - [ ] Update系メソッド実装
  - [ ] 色変化ロジック（警告表示）

- [ ] HUD Prefab 作成
  - [ ] `Assets/Prefabs/HUD.prefab`
  - [ ] MainGameScene に配置

### 4.2 スコア表示画面

- [ ] ScoreDisplay.cs 実装
  - [ ] 衛生スコア表示（TextMeshProUGUI）
  - [ ] 効率スコア表示（TextMeshProUGUI）
  - [ ] リスタートボタン
  - [ ] タイトルに戻るボタン

- [ ] スコアパネルUI作成
  - [ ] Canvas（Screen Space）
  - [ ] Panel（背景）
  - [ ] スコアテキスト配置
  - [ ] ボタン配置

- [ ] GameManager連携
  - [ ] EndGame() でスコア画面表示
  - [ ] ScoreCalculator からスコア取得
  - [ ] ボタンのイベント接続

---

## Phase 5: ゲームフロー実装

### 5.1 ゲーム開始・終了

- [ ] GameManager 拡張
  - [ ] 初期化処理（パラメータリセット）
  - [ ] 開始時のチュートリアル呼び出し
  - [ ] 終了条件判定（体力0、時間制限等）
  - [ ] スコア計算・保存

- [ ] タイトル画面（オプション）
  - [ ] タイトルシーン作成
  - [ ] スタートボタン
  - [ ] 設定ボタン
  - [ ] シーン遷移

### 5.2 チュートリアル

- [ ] TutorialManager.cs 実装
  - [ ] ステップ管理（井戸→飲む→畑）
  - [ ] UI ガイド表示
  - [ ] 完了条件チェック

- [ ] チュートリアルUI
  - [ ] テキストパネル（操作説明）
  - [ ] 矢印・ハイライト（次の目標）
  - [ ] スキップボタン

- [ ] 音声ガイド（オプション）
  - [ ] 音声ファイル準備
  - [ ] AudioSource 再生

---

## Phase 6: テスト・最適化

### 6.1 Quest 実機テスト

- [ ] ビルド設定確認
  - [ ] Platform: Android
  - [ ] Graphics API: OpenGL ES3
  - [ ] Minimum API Level: 29（Android 10）

- [ ] 実機ビルド・インストール
  - [ ] APKビルド
  - [ ] adb install or SideQuest使用
  - [ ] Quest 実機起動

- [ ] パフォーマンステスト
  - [ ] フレームレート計測（目標: 72fps以上）
  - [ ] OVR Metrics Tool 使用
  - [ ] ボトルネック特定

- [ ] インタラクションテスト
  - [ ] 掴み・離しの自然さ
  - [ ] トリガー判定の範囲
  - [ ] UI の視認性

### 6.2 パラメータ調整

- [ ] バランステスト
  - [ ] 各行動の水量変化が適切か
  - [ ] 体力の減少ペースが適切か
  - [ ] ゲーム時間の長さ調整

- [ ] スコア妥当性確認
  - [ ] 衛生スコアの計算式検証
  - [ ] 効率スコアの計算式検証
  - [ ] テストプレイでのスコア分布確認

### 6.3 最適化

- [ ] 描画最適化
  - [ ] 不要なオブジェクト削除
  - [ ] テクスチャ圧縮（ASTC 6x6）
  - [ ] シェーダー最適化

- [ ] スクリプト最適化
  - [ ] Update() の削減
  - [ ] GC Alloc の削減
  - [ ] オブジェクトプール導入

- [ ] アセット削減
  - [ ] 未使用アセット削除
  - [ ] サンプルフォルダ削除
  - [ ] APKサイズ確認

---

## Phase 7: ドキュメント整備

- [ ] README.md 作成
  - [ ] プロジェクト概要
  - [ ] ビルド手順
  - [ ] 操作方法

- [ ] プレイガイド作成
  - [ ] ゲームの目的
  - [ ] 操作説明
  - [ ] スコアリング説明

- [ ] 開発メモ更新
  - [ ] CLAUDE.md に開発履歴追加
  - [ ] 既知の問題・制限事項

---

## 追加機能（オプション）

### CSV出力機能

- [ ] StatisticsTracker.ExportToCSV() 実装
  - [ ] データのCSV形式変換
  - [ ] ファイル保存（Application.persistentDataPath）
  - [ ] エクスポートボタンUI

### 拡張イベント

- [ ] 雨イベント
  - [ ] 水質変動（汚染）
  - [ ] パーティクルエフェクト

- [ ] 時間経過システム
  - [ ] Day/Night サイクル
  - [ ] 時間経過による水質低下

---

## バグ管理

### 既知の問題

_現在なし_

### 修正済み

_現在なし_

---

## 進捗トラッキング

| Phase | 進捗 | 完了予定 |
|:--|:--:|:--:|
| Phase 1: 環境構築 | 0% | Week 2 |
| Phase 2: コアメカニクス | 0% | Week 5 |
| Phase 3: 流体表現 | 0% | Week 6 |
| Phase 4: UI実装 | 0% | Week 7 |
| Phase 5: ゲームフロー | 0% | Week 8 |
| Phase 6: テスト・最適化 | 0% | Week 10 |
| Phase 7: ドキュメント | 0% | Week 10 |

**全体進捗**: 0%（仕様策定完了）

---

**作成日**: 2025-10-25
**最終更新**: 2025-10-25
