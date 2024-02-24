using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using UnityEngine;

public class RecycleParticleOnDeath : MonoBehaviour
{
    public Action OnStopCallback;
    
    private void OnParticleSystemStopped()
    {
        OnStopCallback?.Invoke();
        GameObjectPoolManager.Singleton.Recycle(gameObject);
    }
}
