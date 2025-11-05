# VR Water Resource Serious Game (MVP)

**教育用VRゲーム - 水資源管理シミュレーター**

Meta Quest向けの教育VRゲーム。井戸から水を汲み、生活に使用する体験を通して、水資源の有限性と意思決定の影響を学習します。

---

## 📖 プロジェクト概要

- **プラットフォーム**: Meta Quest (Android)
- **Unity バージョン**: 6000.2.7f2
- **開発期間**: 約10週間（MVP）
- **目的**: 水資源問題の理解促進

### ゲームコンセプト

プレイヤーは農村環境で井戸から水を汲み、以下の選択を行います：

- 🚰 **飲用**: 体力回復（水質が重要）
- 🌾 **農業**: 作物への水やり
- 💧 **廃棄**: 不要な水の処理（環境への影響）

**スコアリング**: 衛生（安全な水使用率）と効率（無駄なく使えたか）の2軸で評価

---

## 📚 ドキュメント

### 必読ドキュメント

| ファイル | 内容 | 対象読者 |
|:--|:--|:--|
| [DEVELOPMENT_GUIDE.md](./docs/DEVELOPMENT_GUIDE.md) | **開発ガイド統合版**（仕様・設計・実装計画） | 全員 |
| [TASKS.md](./docs/TASKS.md) | 開発タスク一覧（チェックリスト） | 開発者 |
| [CLAUDE.md](./CLAUDE.md) | Claude Code 用プロジェクト情報 | AI支援開発 |

---

## 🎮 ゲームメカニクス

### パラメータ管理

| パラメータ | 範囲 | 説明 |
|:--|:--:|:--|
| 水量 | 0-100 | 保持している水の量 |
| 水質 | 0-100 | 水の清潔度（80以上で安全） |
| 体力 | 0-100 | プレイヤーの体力 |

### 器具

- **バケツ** (容量: 80) - 水汲み・農業用
- **コップ** (容量: 10) - 飲用専用

### アクション

| 行動 | 効果 |
|:--|:--|
| 井戸から水を汲む | 水+80, 水質+10, 体力-10 |
| 作物に水をまく | 水-50, 水質-5, 体力-15 |
| 水を飲む | 水-10, 体力±10（水質依存） |
| 水を捨てる | 水-20, 水質-10, 体力-2 |

詳細は [DEVELOPMENT_GUIDE.md](./docs/DEVELOPMENT_GUIDE.md) を参照

---

## 🛠️ 技術スタック

### VR Framework

- **Meta XR SDK** v78.0.0（**ネイティブAPI使用**）
  - OVRCameraRig - VRカメラシステム
  - OVRHand - ハンドトラッキング
  - OVRInput - コントローラ入力
  - **注意**: OpenXR ではなく Meta XR Plugin（Oculus）を使用

### Unity パッケージ

- **Obi Fluid** - 流体物理シミュレーション
- **TextMeshPro** - UI表示

### 使用アセット

- **Viking Village** - 環境モデル（井戸、建物、畑）
- **VillagePack** - 追加の村オブジェクト
- **Mugs, Bowls and Plates** - コップモデル

### レンダーパイプライン

**Built-in Render Pipeline** (Quest最適化)

---

## 🚀 開発セットアップ

### 必要要件

- Unity 6000.2.7f2
- Android Build Support
- Meta Quest (実機テスト用)

### プロジェクトを開く

```bash
# Unity Hubでプロジェクトを開く
# File > Open Project > このディレクトリを選択
```

### ビルド設定

```
Platform: Android
Graphics API: OpenGL ES3
Minimum API Level: Android 10
XR Plugin Management: Oculus（Meta XR Plugin）
  ⚠️ OpenXR は無効化または優先度を下げる
Hand Tracking: Enabled
```

詳細は [DEVELOPMENT_GUIDE.md](./docs/DEVELOPMENT_GUIDE.md) の「開発環境セットアップ」を参照

---

## 📋 開発フェーズ

| フェーズ | 内容 | 期間 |
|:--|:--|:--:|
| **Phase 1** | 環境構築・基盤整備 | 1-2週 |
| **Phase 2** | コアメカニクス実装 | 2-3週 |
| **Phase 3** | 流体表現（簡易版） | 1週 |
| **Phase 4** | UI実装 | 1週 |
| **Phase 5** | ゲームフロー実装 | 1週 |
| **Phase 6** | テスト・最適化 | 1-2週 |

**現在の状態**: Phase 0 (仕様策定完了)

進捗詳細は [TASKS.md](./docs/TASKS.md) で管理

---

## 🎯 開発の進め方

### 1. ドキュメントを読む

まず以下を順に読んでください：

1. `docs/DEVELOPMENT_GUIDE.md` - ゲーム仕様・設計・実装計画の統合版
2. `docs/TASKS.md` - 具体的なタスク確認

### 2. Phase 1 から開始

```bash
# docs/TASKS.md の Phase 1 チェックリストを順に実行
# - シーン構築
# - VR インタラクション基盤
```

### 3. 実装しながら更新

- タスク完了時に `docs/TASKS.md` のチェックボックスを更新
- 問題発生時は `docs/TASKS.md` の「バグ管理」セクションに記録
- 設計変更時は `docs/DESIGN.md` を更新

---

## 📁 プロジェクト構造

```
2025/
├── Assets/
│   ├── Scenes/
│   │   └── MainGameScene.unity     # メインシーン（作成予定）
│   ├── Scripts/
│   │   ├── GameManagement/         # ゲーム管理
│   │   ├── WaterSystem/            # 水システム
│   │   ├── Interactions/           # インタラクション
│   │   └── UI/                     # UI
│   ├── Prefabs/                    # プレハブ（作成予定）
│   └── Viking Village/             # 環境アセット
│
├── docs/                           # 📚 ドキュメント
│   ├── DEVELOPMENT_GUIDE.md        # 開発ガイド統合版 ⭐
│   └── TASKS.md                    # タスク管理 ⭐
│
├── CLAUDE.md                       # AI開発支援用
└── README.md                       # このファイル
```

---

## ⚠️ 既知の制限

- Quest のパフォーマンス制約により、Obi Fluidのパーティクル数は最大500個
- 高度な流体シミュレーションは使用しない（教育効果を優先）
- Built-in RPを使用（URP/HDRP未使用）

詳細は `CLAUDE.md` の「Common Build Issues」を参照

---

## 🤝 貢献

このプロジェクトはMVP（最小構成版）です。

拡張機能のアイデア：
- CSV出力機能（行動履歴のデータ分析）
- 雨イベント（水質変動）
- マルチプレイヤー対応
- 地域別水資源データとの連動

詳細は `docs/DEVELOPMENT_GUIDE.md` の「付記」を参照

---

## 📜 ライセンス

教育・研究用途での使用を想定

使用アセットのライセンス：
- Viking Village: Unity Asset Store
- Obi Fluid: Virtual Method
- Meta XR SDK: Meta Platforms

---

## 📞 サポート

開発に関する質問や問題：

1. まず関連ドキュメントを確認
2. `docs/TASKS.md` の「バグ管理」セクションに記録
3. `CLAUDE.md` を参照してClaude Codeで質問

---

**作成日**: 2025-10-25
**最終更新**: 2025-10-25
**バージョン**: MVP 1.0
