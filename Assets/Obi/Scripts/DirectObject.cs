using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtObject : MonoBehaviour
{
    [Range(0, 100)]
    public float dirtLevel = 100f;

    public void Clean(float amount)
    {
        dirtLevel -= amount;
        dirtLevel = Mathf.Clamp(dirtLevel, 0, 100);
        Debug.Log($"{gameObject.name} ‚Ì‰˜‚êƒŒƒxƒ‹: {dirtLevel}");
    }

    public void Dirty(float amount)
    {
        dirtLevel += amount;
        dirtLevel = Mathf.Clamp(dirtLevel, 0, 100);
        Debug.Log($"{gameObject.name} ‚Ì‰˜‚êƒŒƒxƒ‹: {dirtLevel}");
    }
 
}
