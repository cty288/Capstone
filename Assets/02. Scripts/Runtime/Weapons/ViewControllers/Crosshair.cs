using System;
using System.Linq;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
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
using Runtime.Weapons.ViewControllers.CrossHairs;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime.Weapons.ViewControllers {
    public struct CrosshairGroundWallHitInfo {
        public bool IsHit;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public Vector3 LookDirection;
        public GameObject HitGameObject;
        
        public void Reset() {
            IsHit = false;
            HitPoint = Vector3.zero;
            HitNormal = Vector3.zero;
            LookDirection = Vector3.zero;
            HitGameObject = null;
        }
        
        public void Set(RaycastHit hit, Vector3 lookDirection, GameObject hitGameObject) {
            IsHit = true;
            HitPoint = hit.point;
            HitNormal = hit.normal;
            LookDirection = lookDirection;
            HitGameObject = hitGameObject;
        }
    }
    public class Crosshair : MonoMikroSingleton<Crosshair>, IController {
       // private Transform centerTransform;
        private Camera mainCamera;  
        [SerializeField] 
        private float rayDistance = 100f;
        private ICrossHairDetectable currentPointedObject;
        private IHurtbox currentPointedHurtbox;
        [SerializeField]
        private LayerMask detectLayerMask;
        
        private LayerMask crossHairDetectLayerMask;
        private LayerMask hurtboxLayerMask;

        public ICrossHairDetectable CurrentPointedObject => currentPointedObject;
    
        private RaycastHit[] hits = new RaycastHit[20];
        private RaycastHit[] groundWallhits = new RaycastHit[20];

        [SerializeField]
        private LayerMask wallLayerMask;
        
        private IInventoryModel inventoryModel;
        
        private GameObject noweaponCrosshair;

        private GameObject currentCrosshair;
        private ICrossHairViewController currentCrosshairViewController;
        
        private Vector2 crossHairScreenPosition;

        public Vector2 CrossHairScreenPosition => crossHairScreenPosition;

        //public CrosshairGroundWallHitInfo GroundWallHitInfo { get; } = new CrosshairGroundWallHitInfo();

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

            
            //clear hitsA
            for (int i = 0; i < hits.Length; i++) {
                hits[i] = new RaycastHit();
            }

            int numHits =
                Physics.RaycastNonAlloc(ray, hits, rayDistance, detectLayerMask, QueryTriggerInteraction.Collide);
            var sortedHits = hits.OrderBy(hit => hit.transform ? hit.distance : float.MaxValue).ToArray();

            Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red);
            //GroundWallHitInfo.Reset();
            
            for (int i = 0; i < sortedHits.Length; i++) {
                if (!sortedHits[i].collider) {
                    continue;
                }
                GameObject hitObj = sortedHits[i].collider.gameObject;
                ICrossHairDetectable entityViewController = hitObj.GetComponentInParent<ICrossHairDetectable>();
                
                if (!sortedHits[i].collider.isTrigger && PhysicsUtility.IsInLayerMask(hitObj, wallLayerMask) && entityViewController == null) {
                    //add ground & wall hit info
                    //GroundWallHitInfo.Set(sortedHits[i], ray.direction.normalized, hitObj);
                    break;
                }
                
                //PhysicsUtility.IsInLayerMask(hitObj, crossHairDetectLayerMask)
                if(!hitEntity && entityViewController != null) {
                   
                    if (currentPointedObject != null && currentPointedObject != entityViewController) {
                        currentPointedObject.OnUnPointByCrosshair(); //TODO: change to ICrosshairDetectable
                        currentPointedObject = null;
                    }

                    if (currentPointedObject == null) {
                        currentPointedObject = entityViewController;
                        entityViewController.OnPointByCrosshair();
                    }
                    hitEntity = true;
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
                if (currentPointedObject as Object != null) {
                    currentPointedObject.OnUnPointByCrosshair();
                    currentPointedObject = null;
                }
            }
            
            if (!hitHurtBox) {
                if (currentPointedHurtbox as Object != null) {
                    currentCrosshairViewController?.OnAimHurtBoxExit(currentPointedHurtbox);
                    currentPointedHurtbox = null;
                }
            }
        }
        
        public CrosshairGroundWallHitInfo GetGroundWallHitInfoFromCrosshair(float rayDistance, LayerMask obstrustionLayerMask) {
            CrosshairGroundWallHitInfo hitInfo = new CrosshairGroundWallHitInfo();
            if (!mainCamera) {
                return hitInfo;
            }

            crossHairScreenPosition = currentCrosshair
                ? currentCrosshair.transform.position
                : new Vector2(Screen.width / 2f, Screen.height / 2f);


            Ray ray = mainCamera.ScreenPointToRay(CrossHairScreenPosition);
            bool hitEntity = false;
            bool hitHurtBox = false;

            
            //clear hits
            for (int i = 0; i < groundWallhits.Length; i++) {
                groundWallhits[i] = new RaycastHit();
            }
            int numHits = Physics.RaycastNonAlloc(ray, groundWallhits, rayDistance, obstrustionLayerMask);
            var sortedHits = groundWallhits.OrderBy(hit => hit.transform ? hit.distance : float.MaxValue).ToArray();

            //GroundWallHitInfo.Reset();

            for (int i = 0; i < sortedHits.Length; i++) {
                if (!sortedHits[i].collider || sortedHits[i].collider.isTrigger) {
                    continue;
                }

                GameObject hitObj = sortedHits[i].collider.gameObject;

                if (PhysicsUtility.IsInLayerMask(hitObj, obstrustionLayerMask)) {
                    //add ground & wall hit info
                    hitInfo.Set(sortedHits[i], ray.direction.normalized, hitObj);
                    return hitInfo;
                }
            }
            
            hitInfo.HitNormal = Vector3.up;
            hitInfo.HitPoint = ray.origin + ray.direction * rayDistance;
            return hitInfo;
        }

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }
    }
}
