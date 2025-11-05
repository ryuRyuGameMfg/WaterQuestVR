using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;

public class WaterCollisionHandler : MonoBehaviour
{
    public string cleanWaterEmitterName = "CleanWaterEmitter";
    public string dirtyWaterEmitterName = "DirtyWaterEmitter";

    void OnParticleCollision(GameObject other)
    {
        ObiEmitter emitter = other.GetComponent<ObiEmitter>();
        DirtObject dirt = GetComponent<DirtObject>();

        if (emitter != null && dirt != null)
        {
            if (emitter.name == cleanWaterEmitterName)
            {
                dirt.Clean(10f); // ‰˜‚ê‚ğŒ¸‚ç‚·
            }
            else if (emitter.name == dirtyWaterEmitterName)
            {
                dirt.Dirty(5f); // ‰˜‚ê‚ª‚³‚ç‚É‘‚¦‚éi”CˆÓj
            }
        }
    }
}


