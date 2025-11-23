using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterContainer : MonoBehaviour
{
    [Header("最大容量 (L)")]
    public float maxLiters = 5f;

    [Header("現在量 (L)")]
    public float currentLiters = 0f;

    [Header("水質 0=汚い, 100=きれい")]
    [Range(0, 100)]
    public float quality = 0f;

    public void AddWater(float liters, float incomingQuality)
    {
        float total = currentLiters + liters;
        if (total > maxLiters) total = maxLiters;
        // 水の品質の重み付き平均
        quality = (quality * currentLiters + incomingQuality * liters) / Mathf.Max(total, 0.0001f);
        currentLiters = total;
    }

    public void DirtyByUse(float delta = 10f)
    {
        quality = Mathf.Clamp(quality + delta, 0, 100);
    }

    public float Consume(float liters)
    {
        float used = Mathf.Min(liters, currentLiters);
        currentLiters -= used;
        return used;
    }
}
