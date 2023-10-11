using System;
using System.Linq;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.Singletons;
using MikroFramework.Utilities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime.Weapons.ViewControllers {
    public class Crosshair : MonoMikroSingleton<Crosshair>, IController {
       // private Transform centerTransform;
        private Camera mainCamera;  
        [SerializeField] 
        private float rayDistance = 100f;
        private IEntityViewController currentPointedEntity;
        private IHurtbox currentPointedHurtbox;
        [SerializeField]
        private LayerMask detectLayerMask;
        
        private LayerMask crossHairDetectLayerMask;
        private LayerMask hurtboxLayerMask;

        public IEntityViewController CurrentPointedEntity => currentPointedEntity;
    
        private RaycastHit[] hits = new RaycastHit[20];

        [SerializeField]
        private LayerMask wallLayerMask;
        
        private IInventoryModel inventoryModel;
        
        private GameObject noweaponCrosshair;

        private GameObject currentCrosshair;
        private ICrossHairViewController currentCrosshairViewController;
        
        private Vector2 crossHairScreenPosition;

        public Vector2 CrossHairScreenPosition => crossHairScreenPosition;
        
        private void Awake() {
            inventoryModel = this.GetModel<IInventoryModel>();
            //centerTransform = transform.Find("Center");
            mainCamera = Camera.main;
            crossHairDetectLayerMask = LayerMask.GetMask("CrossHairDetect");
            hurtboxLayerMask = LayerMask.GetMask("Hurtbox");
            noweaponCrosshair = transform.Find("NoWeaponCrossHair").gameObject;
            noweaponCrosshair.SetActive(true);
            
            
        }

 

        public ICrossHairViewController SpawnCrossHair(string prefabName) {
            if (currentCrosshair) {
                GameObjectPoolManager.Singleton.Recycle(currentCrosshair);
                currentCrosshair = null;
                currentCrosshairViewController = null;
            }
            if (String.IsNullOrEmpty(prefabName)) {
                noweaponCrosshair.SetActive(true);
                return null;
            }
            
            noweaponCrosshair.SetActive(false);
            currentCrosshair = GameObjectPoolManager.Singleton.CreatePoolFromAB(prefabName, null, 1, 5, out _)
                .Allocate();
            currentCrosshair.transform.SetParent(transform);
            currentCrosshair.transform.localPosition = Vector3.zero;
            currentCrosshair.transform.localRotation = Quaternion.identity;
            currentCrosshair.transform.localScale = Vector3.one;
            currentCrosshairViewController = currentCrosshair.GetComponent<ICrossHairViewController>();

            return currentCrosshairViewController;
        }
        
        
        public void DespawnCrossHair() {
            if (currentCrosshair) {
                GameObjectPoolManager.Singleton.Recycle(currentCrosshair);
                currentCrosshair = null;
                currentCrosshairViewController = null;
            }

            noweaponCrosshair.SetActive(true);
        }

        private void Update()
        {
            if (!mainCamera) {
                return;
            }

            crossHairScreenPosition = currentCrosshair
                ? currentCrosshair.transform.position
                : new Vector2(Screen.width / 2f, Screen.height / 2f);


            Ray ray = mainCamera.ScreenPointToRay(CrossHairScreenPosition);
            bool hitEntity = false;
            bool hitHurtBox = false;

            
            //clear hits
            for (int i = 0; i < hits.Length; i++) {
                hits[i] = new RaycastHit();
            }
            int numHits = Physics.RaycastNonAlloc(ray, hits, rayDistance, detectLayerMask);
            var sortedHits = hits.OrderBy(hit => hit.transform ? hit.distance : float.MaxValue).ToArray();

            for (int i = 0; i < sortedHits.Length; i++) {
                if (!sortedHits[i].collider) {
                    continue;
                }
                GameObject hitObj = sortedHits[i].collider.gameObject;
                
                if (PhysicsUtility.IsInLayerMask(hitObj, wallLayerMask)) {
                    break;
                }
                
                if(!hitEntity && PhysicsUtility.IsInLayerMask(hitObj, crossHairDetectLayerMask)) {
                    if (hitObj.transform.parent.TryGetComponent<IEntityViewController>(out var entityViewController)){
                        
                        if (currentPointedEntity != null && currentPointedEntity != entityViewController) {
                            currentPointedEntity.OnUnPointByCrosshair();
                            currentPointedEntity = null;
                        }

                        if (currentPointedEntity == null) {
                            currentPointedEntity = entityViewController;
                            entityViewController.OnPointByCrosshair();
                        }
                        hitEntity = true;
                    }
                }

                if (!hitHurtBox && PhysicsUtility.IsInLayerMask(hitObj, hurtboxLayerMask)) {
                    if (hitObj.TryGetComponent<IHurtbox>(out var hurtbox)){
                        
                        if (currentPointedHurtbox != null && currentPointedHurtbox != hurtbox) {
                            currentCrosshairViewController?.OnAimHurtBoxExit(currentPointedHurtbox);
                            currentPointedHurtbox = null;
                        }

                        if (currentPointedHurtbox == null) {
                            currentPointedHurtbox = hurtbox;
                            currentCrosshairViewController?.OnAimHurtBoxEnter(currentPointedHurtbox);
                        }
                        
                        hitHurtBox = true;
                    }
                }
            }
            
            if (!hitEntity) {
                if (currentPointedEntity as Object != null) {
                    currentPointedEntity.OnUnPointByCrosshair();
                    currentPointedEntity = null;
                }
            }
            
            if (!hitHurtBox) {
                if (currentPointedHurtbox as Object != null) {
                    currentCrosshairViewController?.OnAimHurtBoxExit(currentPointedHurtbox);
                    currentPointedHurtbox = null;
                }
            }
        }

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }
    }
}
