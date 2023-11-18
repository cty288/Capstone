using System;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Pool;
using MikroFramework.Singletons;
using UnityEngine;

namespace Runtime.UI.NameTags {
    public class HUDElementInfo {
        public Dictionary<Transform, (GameObject, bool)> followDict = new Dictionary<Transform, (GameObject, bool)>();
        
        public Dictionary<string, SafeGameObjectPool> prefabPools = new Dictionary<string, SafeGameObjectPool>();
        
        public HUDElementInfo() {
            
        }

        
        
        public (GameObject, bool) GetOrCreate(Transform targetFollow, Transform spawnedTransform, string prefabName, bool isWorld) {
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
            followDict.Add(targetFollow, (nameTag, isWorld));
            return (nameTag, isWorld);
        }
    
        public void Despawn(Transform targetFollow) {
            if (followDict.ContainsKey(targetFollow)) {
                GameObjectPoolManager.Singleton.Recycle(followDict[targetFollow].Item1);
                followDict.Remove(targetFollow);
            }
        }
        
        public void ClearAll() {
            foreach (var pair in followDict) {
                GameObjectPoolManager.Singleton.Recycle(pair.Value.Item1);
            }
            followDict.Clear();
        }
    }

    public enum HUDCategory {
        NameTag,
        InteractiveTag,
        SlotDescription,
        HealthBar,
        Exit
    }
    
    public class HUDManager : MonoBehaviour, ISingleton {

        private Dictionary<HUDCategory, HUDElementInfo> hudElementInfos = new Dictionary<HUDCategory, HUDElementInfo>();
        private Camera mainCamera;


        public static HUDManager Singleton {
            get {
                return SingletonProperty<HUDManager>.Singleton;
            }
        }

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
        public GameObject SpawnHUDElement(Transform targetFollow, string prefabName, HUDCategory hudCategory, bool isWorld) {
            if (!hudElementInfos.ContainsKey(hudCategory)) {
                hudElementInfos.Add(hudCategory, new HUDElementInfo());
            }

            return hudElementInfos[hudCategory].GetOrCreate(targetFollow, transform, prefabName, isWorld).Item1;
        }
    
        public void DespawnHUDElement(Transform targetFollow, HUDCategory hudCategory) {
            if (hudElementInfos.TryGetValue(hudCategory, out var info)) {
                info.Despawn(targetFollow);
            }
        }


        public void ClearAll() {
            foreach (HUDCategory category in hudElementInfos.Keys) {
                hudElementInfos[category].ClearAll();
            }

            hudElementInfos.Clear();
        }
        public bool HasHUDElement(Transform targetFollow, HUDCategory hudCategory) {
            if (hudElementInfos.TryGetValue(hudCategory, out var info)) {
                return info.followDict.ContainsKey(targetFollow);
            }

            return false;
        }

  

        private void Update() {
            if (!mainCamera) {
                return;
            }

            List<Tuple<Transform, HUDCategory>> toRemove = new List<Tuple<Transform, HUDCategory>>();
            Dictionary<Vector3, Vector3> screenPosDict = new Dictionary<Vector3, Vector3>();
            foreach (var hudElementInfo in hudElementInfos) {
                foreach (KeyValuePair<Transform,(GameObject,bool)> ele in hudElementInfo.Value.followDict) {
                    if (!ele.Key) {
                        //DespawnHUDElement(ele.Key, hudElementInfo.Key);
                        toRemove.Add(new Tuple<Transform, HUDCategory>(ele.Key, hudElementInfo.Key));
                        continue;
                    }

                    if (ele.Key.gameObject.activeInHierarchy) {
                        var position = ele.Key.position;
                        Vector3 screenPos = Vector3.zero;
                        if (ele.Value.Item2) { //is world
                            if (!screenPosDict.ContainsKey(position)) {
                                screenPosDict.Add(position, mainCamera.WorldToScreenPoint(position));
                            }
                            screenPos = screenPosDict[position];
                        }
                        else {
                            screenPos = position;
                        }
                        //set screen pos z to 0
                        if (screenPos.z < 0) {
                            //make x and y negative, so that the element will be hidden
                            screenPos.x = -10000;
                            screenPos.y = -10000;
                        }
                        else {
                            screenPos.z = 0;
                        }
                        
                       
                       
                       
                        ele.Value.Item1.transform.position = screenPos;
                    }
                }
            }
        
            foreach (var tuple in toRemove) {
                DespawnHUDElement(tuple.Item1, tuple.Item2);
            }
        }

        public void OnSingletonInit() {
            
        }
    }
}