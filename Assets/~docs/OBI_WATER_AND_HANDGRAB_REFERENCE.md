# Obi水エフェクト & Meta Quest HandGrab API リファレンス

> **作成日**: 2025-11-26
> **対象**: WaterQuestVR プロジェクト
> **Unity Version**: 2022.3.x
> **Meta XR SDK**: 69.0以降

---

## 📑 目次

1. [Obi水エフェクトシステム](#1-obi水エフェクトシステム)
2. [コリジョン検出とMeshCollider](#2-コリジョン検出とmeshcollider)
3. [Meta Quest HandGrab API](#3-meta-quest-handgrab-api)
4. [実装例とベストプラクティス](#4-実装例とベストプラクティス)

---

## 1. Obi水エフェクトシステム

### 1.1 コアコンポーネント

#### ObiSolver
- **役割**: パーティクルベースの物理シミュレーションエンジン
- **機能**:
  - リアルタイム流体シミュレーション
  - 双方向リジッドボディ相互作用
  - GPU (Compute) / CPU (Burst) バックエンド対応

```csharp
// 必須設定
ObiSolver solver = GetComponent<ObiSolver>();
solver.substeps = 4;              // シミュレーション精度
solver.maxParticleContacts = 6;   // パーティクルあたりの最大接触数
```

#### ObiCollider / ObiCollider2D
- **役割**: Unity Colliderをパーティクルシステムに認識させる
- **必須**: 通常のUnity Colliderだけでは不十分

**サポートされるコライダー**:
- ✅ SphereCollider
- ✅ BoxCollider
- ✅ CapsuleCollider
- ✅ **MeshCollider** (凹型対応)
- ✅ TerrainCollider
- ✅ CharacterController

```csharp
GameObject bowl = /* 受け皿オブジェクト */;
bowl.AddComponent<MeshCollider>();
bowl.AddComponent<ObiCollider>();
```

#### ObiRigidbody
- **役割**: リジッドボディとパーティクルの双方向物理相互作用
- **自動追加**: ObiColliderが親にRigidbodyを検出すると自動で追加

```csharp
// 設定例
ObiRigidbody obiRb = GetComponent<ObiRigidbody>();
obiRb.kinematicForParticles = false; // パーティクルから力を受ける
```

---

## 2. コリジョン検出とMeshCollider

### 2.1 MeshColliderの完全サポート

**✅ Meshに沿った正確な当たり判定**

Obiは**TriangleMeshShape**として実装:
- 実際の頂点・三角形データを使用
- BIH (Bounding Interval Hierarchy) で効率的な衝突検出
- **凹型メッシュ対応**（通常のUnity物理では制限あり）

```csharp
// MeshColliderの設定
MeshCollider meshCollider = bowl.AddComponent<MeshCollider>();
meshCollider.sharedMesh = bowl.GetComponent<MeshFilter>().sharedMesh;
meshCollider.convex = false;  // 凹型でもOK！
```

### 2.2 ObiColliderの重要プロパティ

```csharp
ObiCollider obiCollider = bowl.GetComponent<ObiCollider>();

// 受け皿の内側で衝突を検出
obiCollider.Inverted = true;  // ★重要★

// 衝突範囲の調整
obiCollider.Thickness = 0.01f;

// 物理マテリアル
obiCollider.CollisionMaterial = myCollisionMaterial;
```

#### 受け皿に必須の設定
```csharp
// ボウル、カップなどの受け皿
obiCollider.Inverted = true;  // 内側の衝突を有効化
```

### 2.3 コリジョンイベント

```csharp
public class WaterBowlHandler : MonoBehaviour
{
    private ObiSolver solver;

    void OnEnable()
    {
        solver = GetComponent<ObiSolver>();
        solver.OnCollision += Solver_OnCollision;
    }

    void OnDisable()
    {
        solver.OnCollision -= Solver_OnCollision;
    }

    void Solver_OnCollision(object sender, ObiNativeContactList contacts)
    {
        var colliderWorld = ObiColliderWorld.GetInstance();

        for (int i = 0; i < contacts.count; ++i)
        {
            var contact = contacts[i];

            // 接触距離のチェック
            if (contact.distance < 0.01f)
            {
                // コライダー情報の取得
                var collider = colliderWorld.colliderHandles[contact.bodyB].owner;

                // パーティクルインデックス
                int particleIndex = solver.simplices[contact.bodyA];

                // 処理...
            }
        }
    }
}
```

### 2.4 ObiCollisionMaterial

```csharp
// Assets/Resources/WaterBowlMaterial.asset
var material = ScriptableObject.CreateInstance<ObiCollisionMaterial>();
material.dynamicFriction = 0.3f;   // 動摩擦
material.staticFriction = 0.4f;    // 静摩擦
material.stickiness = 0.1f;        // 粘着性
material.stickDistance = 0.01f;    // 粘着距離
```

---

## 3. Meta Quest HandGrab API

### 3.1 HandGrab システム概要

**階層構造**:
```
GrabbableObject (親)
├─ Rigidbody ★必須★
├─ Collider ★必須★
├─ PointableElement (自動追加)
├─ HandGrabInteractable
├─ DistanceHandGrabInteractable
└─ [BuildingBlock] HandGrabInstallationRoutine (子オブジェクト)
```

### 3.2 必須コンポーネント

#### Rigidbody
```csharp
Rigidbody rb = gameObject.AddComponent<Rigidbody>();
rb.mass = 1.0f;
rb.useGravity = true;
rb.isKinematic = false; // 掴んでいない時
```

#### PointableElement
```csharp
// HandGrabInteractableが自動的に参照
PointableElement pointable = gameObject.AddComponent<PointableElement>();
```

#### HandGrabInteractable
```csharp
HandGrabInteractable grabInteractable = gameObject.AddComponent<HandGrabInteractable>();
grabInteractable.Rigidbody = rb;
grabInteractable.PointableElement = pointable;
grabInteractable.InjectRigidbody(rb);
grabInteractable.InjectPointableElement(pointable);
```

### 3.3 BuildingBlock の使い方

**❌ 間違った使い方**:
```
掴めるオブジェクトに直接HandGrabInstallationRoutineをアタッチ
```

**✅ 正しい使い方**:
```
1. Window > Meta > BuildingBlocks を開く
2. "Hand Grab Interaction" を選択
3. 掴みたいオブジェクトにドラッグ＆ドロップ
4. 自動的に子オブジェクトとして配置される
5. 親オブジェクトのコンポーネントを自動参照
```

### 3.4 HandGrabInteractable 設定例

```csharp
// インスペクター設定（Unityファイル内の実例）
_interactorFilters: []
_maxInteractors: -1
_maxSelectingInteractors: -1
_pointableElement: {fileID: 481507807}  // 親から参照
_rigidbody: {fileID: 481507806}          // 親から参照
_kinematicWhileSelected: 1               // 掴んでいる間はKinematic
_throwWhenUnselected: 1                  // 離した時に投げる
```

### 3.5 DistanceHandGrabInteractable

遠距離から掴む機能:

```csharp
DistanceHandGrabInteractable distanceGrab =
    gameObject.AddComponent<DistanceHandGrabInteractable>();

distanceGrab.Rigidbody = rb;
distanceGrab.PointableElement = pointable;

// スコアリング設定（掴みやすさの調整）
distanceGrab.ScoringModifier.PositionRotationWeight = 0.5f;
```

---

## 4. 実装例とベストプラクティス

### 4.1 水を入れられる受け皿の実装

```csharp
using UnityEngine;
using Obi;

public class WaterBowl : MonoBehaviour
{
    [Header("Obi Settings")]
    public ObiSolver solver;

    [Header("Bowl Settings")]
    public float waterCapacity = 100f;
    private float currentWaterAmount = 0f;

    void Start()
    {
        SetupObiCollider();
        SetupHandGrab();
    }

    void SetupObiCollider()
    {
        // MeshCollider
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        meshCollider.convex = false;

        // ObiCollider
        ObiCollider obiCollider = gameObject.AddComponent<ObiCollider>();
        obiCollider.Inverted = true;  // 内側の衝突
        obiCollider.Thickness = 0.01f;

        // ObiCollisionMaterial
        ObiCollisionMaterial material =
            ScriptableObject.CreateInstance<ObiCollisionMaterial>();
        material.dynamicFriction = 0.3f;
        material.stickiness = 0.05f;
        obiCollider.CollisionMaterial = material;
    }

    void SetupHandGrab()
    {
        // Rigidbody
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.useGravity = true;

        // BuildingBlockを使用する場合は手動不要
        // Window > Meta > BuildingBlocks から追加
    }

    void OnEnable()
    {
        if (solver != null)
            solver.OnCollision += OnWaterCollision;
    }

    void OnDisable()
    {
        if (solver != null)
            solver.OnCollision -= OnWaterCollision;
    }

    void OnWaterCollision(object sender, ObiNativeContactList contacts)
    {
        var colliderWorld = ObiColliderWorld.GetInstance();

        for (int i = 0; i < contacts.count; ++i)
        {
            if (contacts[i].distance < 0.01f)
            {
                var collider = colliderWorld.colliderHandles[contacts[i].bodyB].owner;

                if (collider.gameObject == gameObject)
                {
                    // 水が受け皿に入った
                    currentWaterAmount += 0.1f; // パーティクルごと
                    currentWaterAmount = Mathf.Min(currentWaterAmount, waterCapacity);
                }
            }
        }
    }
}
```

### 4.2 水を注ぐバケツの実装

```csharp
using UnityEngine;
using Obi;

public class WaterBucket : MonoBehaviour
{
    [Header("References")]
    public ObiEmitter waterEmitter;
    public Transform pourPoint;

    [Header("Pour Settings")]
    public float pourAngleThreshold = 45f;
    private bool isPouring = false;

    void Update()
    {
        CheckPouringAngle();
    }

    void CheckPouringAngle()
    {
        // バケツの傾き角度を計算
        float angle = Vector3.Angle(transform.up, Vector3.up);

        if (angle > pourAngleThreshold)
        {
            if (!isPouring)
            {
                StartPouring();
            }
        }
        else
        {
            if (isPouring)
            {
                StopPouring();
            }
        }
    }

    void StartPouring()
    {
        isPouring = true;
        if (waterEmitter != null)
        {
            waterEmitter.speed = 2.0f;
            waterEmitter.lifespan = 5.0f;
        }
    }

    void StopPouring()
    {
        isPouring = false;
        if (waterEmitter != null)
        {
            waterEmitter.speed = 0f;
        }
    }
}
```

### 4.3 パフォーマンス最適化

#### Obi設定
```csharp
// ObiSolver最適化
solver.substeps = 3;                    // 精度とパフォーマンスのバランス
solver.maxParticleContacts = 4;         // 接触数を制限
solver.maxParticleNeighbors = 64;       // 近傍パーティクル数

// MeshColliderの三角形数を削減
// Blenderなどで低ポリゴン版を作成
```

#### Quest最適化
```csharp
// 描画負荷軽減
ObiFluidRenderer renderer = GetComponent<ObiFluidRenderer>();
renderer.particleRendering = false;     // 必要に応じて無効化

// 物理更新頻度の調整
solver.maxStepsPerFrame = 1;            // フレームあたりの最大ステップ数
```

---

## 5. トラブルシューティング

### 5.1 水が受け皿をすり抜ける

**原因**: `Inverted`が設定されていない

**解決策**:
```csharp
ObiCollider obiCollider = bowl.GetComponent<ObiCollider>();
obiCollider.Inverted = true;  // これを設定
```

### 5.2 掴めない

**チェックリスト**:
- [ ] Rigidbodyがアタッチされているか
- [ ] Colliderがアタッチされているか
- [ ] HandGrabInteractableが正しく設定されているか
- [ ] PointableElementが参照されているか

### 5.3 水のパフォーマンスが悪い

**対策**:
1. MeshColliderの三角形数を減らす
2. `solver.substeps`を下げる (3-4推奨)
3. パーティクル数を制限
4. `maxParticleContacts`を減らす

---

## 6. リファレンスリンク

### Obi
- [公式ドキュメント](http://obi.virtualmethodstudio.com/manual/6.3/index.html)
- [API Reference](http://obi.virtualmethodstudio.com/api/index.html)

### Meta Quest
- [BuildingBlocks](https://developer.oculus.com/documentation/unity/bb-overview/)
- [Interaction SDK](https://developer.oculus.com/documentation/unity/unity-isdk-interaction-sdk-overview/)
- [HandGrab Documentation](https://developer.oculus.com/documentation/unity/unity-isdk-hand-grab/)

---

## 7. クイックリファレンス

### 受け皿セットアップ (3ステップ)
```bash
1. MeshCollider追加 (convex=false)
2. ObiCollider追加 (Inverted=true)
3. BuildingBlock追加 (Hand Grab Interaction)
```

### 必須コンポーネントチェックリスト

**水を受ける受け皿**:
- ✅ MeshCollider
- ✅ ObiCollider (Inverted=true)
- ✅ Rigidbody
- ✅ HandGrabInteractable (BuildingBlock経由)

**水を注ぐ器具**:
- ✅ ObiEmitter
- ✅ Rigidbody
- ✅ Collider
- ✅ HandGrabInteractable (BuildingBlock経由)

---

**最終更新**: 2025-11-26
**作成者**: AI Assistant
**プロジェクト**: WaterQuestVR

