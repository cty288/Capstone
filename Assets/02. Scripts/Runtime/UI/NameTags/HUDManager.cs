using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;

public class HUDElementInfo {
    public Dictionary<Transform, GameObject> followDict = new Dictionary<Transform, GameObject>();
    public Dictionary<string, SafeGameObjectPool> prefabPools = new Dictionary<string, SafeGameObjectPool>();
        
    public HUDElementInfo() {
            
    }
        
    public GameObject GetOrCreate(Transform targetFollow, Transform spawnedTransform, string prefabName) {
        if (followDict.ContainsKey(targetFollow)) {
            return followDict[targetFollow];
        }
        
        if (!prefabPools.ContainsKey(prefabName)) {
            prefabPools.Add(prefabName, GameObjectPoolManager.Singleton.CreatePoolFromAB(prefabName, null, 10, 50,
                out GameObject prefab));
        }
        
        GameObject nameTag = prefabPools[prefabName].Allocate();
        nameTag.transform.SetParent(spawnedTransform);
        nameTag.transform.localScale = Vector3.one;
        nameTag.transform.rotation = Quaternion.identity;
        followDict.Add(targetFollow, nameTag);
        return nameTag;
    }
    
    public void Despawn(Transform targetFollow) {
        if (followDict.ContainsKey(targetFollow)) {
            GameObjectPoolManager.Singleton.Recycle(followDict[targetFollow]);
            followDict.Remove(targetFollow);
        }
    }
}

public enum HUDCategory {
    NameTag,
    InteractiveTag
}

public class HUDManager : MonoMikroSingleton<HUDManager> {

    private Dictionary<HUDCategory, HUDElementInfo> hudElementInfos = new Dictionary<HUDCategory, HUDElementInfo>();
    private Camera mainCamera;



    private void Awake() {
        mainCamera = Camera.main;
    }

    
    /// <summary>
    /// Spawn an HUD element and follow the target
    /// </summary>
    /// <param name="targetFollow"></param>
    /// <param name="prefabName">The asset bundle name of the prefab</param>
    /// <param name="hudCategory">The type of the HUD element. No two HUD elements of the same type can follow the same target. <br />
    /// To create a new HUD element type, add a new enum value to <see cref="HUDCategory"/></param>
    /// <returns></returns>
    public GameObject SpawnHUDElement(Transform targetFollow, string prefabName, HUDCategory hudCategory) {
        if (!hudElementInfos.ContainsKey(hudCategory)) {
            hudElementInfos.Add(hudCategory, new HUDElementInfo());
        }

        return hudElementInfos[hudCategory].GetOrCreate(targetFollow, transform, prefabName);
    }
    
    public void DespawnHUDElement(Transform targetFollow, HUDCategory hudCategory) {
        if (hudElementInfos.TryGetValue(hudCategory, out var info)) {
            info.Despawn(targetFollow);
        }
    }

  

    private void Update() {
        if (!mainCamera) {
            return;
        }

        Dictionary<Vector3, Vector3> screenPosDict = new Dictionary<Vector3, Vector3>();
        foreach (HUDElementInfo hudElementInfo in hudElementInfos.Values) {
            foreach (KeyValuePair<Transform,GameObject> ele in hudElementInfo.followDict) {
                if (!ele.Key) {
                    DespawnHUDElement(ele.Key, HUDCategory.NameTag);
                }
                
                var position = ele.Key.position;
                if (!screenPosDict.ContainsKey(position)) {
                    screenPosDict.Add(position, mainCamera.WorldToScreenPoint(position));
                }
                Vector3 screenPos = screenPosDict[position];
                ele.Value.transform.position = screenPos;
            }
        }
    }
}
