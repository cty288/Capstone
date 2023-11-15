using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Singletons;
using Runtime.UI.NameTags;
using UnityEngine;

public class HUDManagerUI : MonoBehaviour, ISingleton
{
        private Dictionary<HUDCategory, HUDElementInfo> hudElementInfos = new Dictionary<HUDCategory, HUDElementInfo>();
        private Camera mainCamera;


        public static HUDManagerUI Singleton {
            get {
                return SingletonProperty<HUDManagerUI>.Singleton;
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
                       
                        ele.Value.Item1.transform.position = screenPos;
                    }
                }
            }
        
            foreach (var tuple in toRemove) {
                DespawnHUDElement(tuple.Item1, tuple.Item2);
            }
        }
        public void ClearAll() {
            foreach (HUDCategory category in hudElementInfos.Keys) {
                hudElementInfos[category].ClearAll();
            }

            hudElementInfos.Clear();
        }
        public void OnSingletonInit() {
            
        }
}
