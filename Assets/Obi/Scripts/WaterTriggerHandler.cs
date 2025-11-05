using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTriggerHandler : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        DirtObject dirt = GetComponentInParent<DirtObject>();

        if (dirt != null)
        {
            dirt.Clean(0.5f); // ‰˜‚ê‚ğ­‚µ‚¸‚ÂŒ¸‚ç‚·
            Debug.Log($"{dirt.gameObject.name} ‚Ì‰˜‚êƒŒƒxƒ‹: {dirt.dirtLevel}");
        }
    }


void Start()
    {
        Debug.Log($"{gameObject.name} ‚Ìwatertrigger‚Í‹N“®‚µ‚Ä‚é‚æI");
    }
}


