using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;

public class NameTagManager : MonoMikroSingleton<NameTagManager> {
    private Dictionary<Transform, GameObject> nameTagDict = new Dictionary<Transform, GameObject>();
    private Dictionary<string, SafeGameObjectPool> nameTagPools = new Dictionary<string, SafeGameObjectPool>();
    private Camera mainCamera;

    private void Awake() {
        mainCamera = Camera.main;
    }

    public GameObject SpawnNameTag(Transform targetFollow, string prefabName) {
        if (nameTagDict.ContainsKey(targetFollow)) {
            return nameTagDict[targetFollow];
        }
        
        if (!nameTagPools.ContainsKey(prefabName)) {
            nameTagPools.Add(prefabName, GameObjectPoolManager.Singleton.CreatePoolFromAB(prefabName, null, 10, 50,
                out GameObject prefab));
        }
        
        GameObject nameTag = nameTagPools[prefabName].Allocate();
        nameTag.transform.SetParent(transform);
        nameTag.transform.localScale = Vector3.one;
        nameTag.transform.rotation = Quaternion.identity;
        nameTagDict.Add(targetFollow, nameTag);
        return nameTag;
    }
    
    public void DespawnNameTag(Transform targetFollow) {
        if (nameTagDict.ContainsKey(targetFollow)) {
            GameObjectPoolManager.Singleton.Recycle(nameTagDict[targetFollow]);
            nameTagDict.Remove(targetFollow);
        }
    }

    private void Update() {
        if (!mainCamera) {
            return;
        }
        foreach (var nameTag in nameTagDict) {
            if (!nameTag.Key) {
                DespawnNameTag(nameTag.Key);
            }
            Vector3 screenPos = mainCamera.WorldToScreenPoint(nameTag.Key.position);
            nameTag.Value.transform.position = screenPos;
        }
    }
}
