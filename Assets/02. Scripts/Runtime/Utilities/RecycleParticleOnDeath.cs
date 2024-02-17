using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using UnityEngine;

public class RecycleParticleOnDeath : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        GameObjectPoolManager.Singleton.Recycle(gameObject);
    }
}
