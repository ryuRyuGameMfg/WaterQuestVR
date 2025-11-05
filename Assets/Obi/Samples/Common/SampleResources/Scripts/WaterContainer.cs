using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterContainer : MonoBehaviour
{
    [Header("Å‘å—e—Ê (L)")]
    public float maxLiters = 5f;

    [Header("Œ»İ—Ê (L)")]
    public float currentLiters = 0f;

    [Header("…¿ 0=‚«‚ê‚¢, 100=‰˜‚¢")]
    [Range(0, 100)]
    public float quality = 0f;

    public void AddWater(float liters, float incomingQuality)
    {
        float total = currentLiters + liters;
        if (total > maxLiters) total = maxLiters;
        // ¿‚Ì¬‡F‘ÌÏ‰Ád•½‹Ï
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
