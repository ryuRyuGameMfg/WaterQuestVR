using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// シンプルなグリッド配置システム
/// 親オブジェクトにアタッチすると、子オブジェクトを自動検出してグリッド上に配置します
/// 親オブジェクトの位置は変更せず、子オブジェクトのワールド位置を直接変更します
/// </summary>
public class SimpleGridPlacer : MonoBehaviour
{
    [Header("グリッド設定")]
    [Tooltip("オブジェクト間の間隔倍率（1.2 = オブジェクトサイズの1.2倍の間隔）")]
    [SerializeField] private float spacingMultiplier = 1.2f;

    [Tooltip("親オブジェクトからのオフセット（親オブジェクトの位置を基準にした追加オフセット）")]
    [SerializeField] private Vector3 offsetFromParent = Vector3.zero;

    [Header("高さ設定")]
    [Tooltip("高さの配置方法")]
    [SerializeField] private HeightMode heightMode = HeightMode.同じ高さ;

    [Tooltip("基準となる高さ")]
    [SerializeField] private float heightOffset = 0f;

    [Header("レイアウト設定")]
    [Tooltip("1行に配置する最大オブジェクト数（0 = 無制限）")]
    [SerializeField] private int maxObjectsPerRow = 0;

    [Tooltip("最大オブジェクト数（0 = 無制限、パフォーマンス調整用）")]
    [SerializeField] private int maxObjects = 0;

    /// <summary>
    /// 高さの配置方法
    /// </summary>
    public enum HeightMode
    {
        同じ高さ,      // すべて同じ高さで配置
        段階的に増加   // 各オブジェクトごとに高さを増加
    }

    private List<GameObject> childrenObjects = new List<GameObject>();
    private Vector3 maxObjectSize = Vector3.zero;
    private Vector3 calculatedSpacing = Vector3.zero;

    private int currentX = 0;
    private int currentZ = 0;
    private int currentHeight = 0;
    private int lastChildCount = 0;

#if UNITY_EDITOR
    /// <summary>
    /// エディター上で子オブジェクトが変更されたときに呼ばれる
    /// </summary>
    private void OnTransformChildrenChanged()
    {
        // 子オブジェクトの数が変わった場合、再検出
        if (transform.childCount != lastChildCount)
        {
            lastChildCount = transform.childCount;
            DetectChildren();
        }
    }
#endif

    /// <summary>
    /// 子オブジェクトを検出
    /// </summary>
    public void DetectChildren()
    {
        childrenObjects.Clear();

        int count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            // 最大オブジェクト数のチェック
            if (maxObjects > 0 && count >= maxObjects)
            {
                Debug.LogWarning($"[SimpleGridPlacer] 最大オブジェクト数（{maxObjects}）に達しました。それ以降のオブジェクトは無視されます。");
                break;
            }

            GameObject child = transform.GetChild(i).gameObject;
            if (child.activeInHierarchy)
            {
                childrenObjects.Add(child);
                count++;
            }
        }

        lastChildCount = transform.childCount;
        CalculateObjectSizes();

        Debug.Log($"[SimpleGridPlacer] {childrenObjects.Count}個の子オブジェクトを検出しました");
    }

    /// <summary>
    /// オブジェクトのサイズを計算
    /// </summary>
    private void CalculateObjectSizes()
    {
        maxObjectSize = Vector3.zero;

        foreach (var obj in childrenObjects)
        {
            Bounds bounds = GetObjectBounds(obj);
            maxObjectSize.x = Mathf.Max(maxObjectSize.x, bounds.size.x);
            maxObjectSize.y = Mathf.Max(maxObjectSize.y, bounds.size.y);
            maxObjectSize.z = Mathf.Max(maxObjectSize.z, bounds.size.z);
        }

        calculatedSpacing = maxObjectSize * spacingMultiplier;
    }

    /// <summary>
    /// オブジェクトのバウンディングボックスを取得
    /// </summary>
    private Bounds GetObjectBounds(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null) return renderer.bounds;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                bounds.Encapsulate(r.bounds);
            }
            return bounds;
        }

        Collider collider = obj.GetComponent<Collider>();
        if (collider != null) return collider.bounds;

        return new Bounds(obj.transform.position, Vector3.one);
    }

    /// <summary>
    /// すべての子オブジェクトをグリッド上に配置
    /// 親オブジェクトの位置は変更せず、子オブジェクトのワールド位置を直接変更します
    /// </summary>
    [ContextMenu("すべてのオブジェクトを配置")]
    public void PlaceAllObjectsOnGrid()
    {
        // 毎回子オブジェクトを再検出（後から追加されたオブジェクトも検出するため）
        DetectChildren();

        if (childrenObjects.Count == 0)
        {
            Debug.LogWarning("[SimpleGridPlacer] 配置する子オブジェクトが見つかりません");
            return;
        }

        CalculateObjectSizes();

        // 親オブジェクトの現在位置を基準位置として使用
        Vector3 basePosition = transform.position + offsetFromParent;

        currentX = 0;
        currentZ = 0;
        currentHeight = 0;

        // 子オブジェクトをグリッド上に配置（親オブジェクトの位置を基準に）
        foreach (var obj in childrenObjects)
        {
            if (obj == null) continue;

            float posX = basePosition.x + currentX * calculatedSpacing.x;
            float posZ = basePosition.z + currentZ * calculatedSpacing.z;
            float posY = heightMode == HeightMode.同じ高さ
                ? basePosition.y + heightOffset
                : basePosition.y + currentHeight * calculatedSpacing.y + heightOffset;

            // ワールド位置で直接配置（親オブジェクトの位置を基準にする）
            obj.transform.position = new Vector3(posX, posY, posZ);

            // 次の位置を計算
            currentX++;
            if (maxObjectsPerRow > 0 && currentX >= maxObjectsPerRow)
            {
                currentX = 0;
                currentZ++;
                if (heightMode == HeightMode.段階的に増加)
                {
                    currentHeight++;
                }
            }
        }
    }

    /// <summary>
    /// 配置をリセット
    /// </summary>
    [ContextMenu("配置をリセット")]
    public void ResetPlacement()
    {
        currentX = 0;
        currentZ = 0;
        currentHeight = 0;
    }
}
