using System.Linq;
using MikroFramework.Singletons;
using MikroFramework.Utilities;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runtime.Weapons.ViewControllers {
    public class Crosshair : MonoMikroSingleton<Crosshair> {
        private Transform centerTransform;
        private Camera mainCamera;  
        [SerializeField] 
        private float rayDistance = 100f;
        private IEntityViewController currentPointedEntity;
        [SerializeField]
        private LayerMask detectLayerMask;
        
        private LayerMask crossHairDetectLayerMask;

        public IEntityViewController CurrentPointedEntity => currentPointedEntity;
    
        private RaycastHit[] hits = new RaycastHit[10];
        
        
        [SerializeField]
        private LayerMask wallLayerMask;
        
        private void Awake() {
            centerTransform = transform.Find("Center");
            mainCamera = Camera.main;
            crossHairDetectLayerMask = LayerMask.GetMask("CrossHairDetect");
        }
    
        private void Update()
        {
            if (!mainCamera) {
                return;
            }
            
            
            Ray ray = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));
            bool hitEntity = false;

            
            //clear hits
            for (int i = 0; i < hits.Length; i++) {
                hits[i] = new RaycastHit();
            }
            int numHits = Physics.RaycastNonAlloc(ray, hits, rayDistance, detectLayerMask);
            var sortedHits = hits.OrderBy(hit => hit.distance).ToArray();

            for (int i = 0; i < sortedHits.Length; i++) {
                if (!sortedHits[i].collider) {
                    continue;
                }
                GameObject hitObj = sortedHits[i].collider.gameObject;
                
                if (PhysicsUtility.IsInLayerMask(hitObj, wallLayerMask)) {
                    break;
                }
                
                if(PhysicsUtility.IsInLayerMask(hitObj, crossHairDetectLayerMask)) {
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
                        break;
                    }
                }
            }
            
            if (!hitEntity) {
                if (currentPointedEntity as Object != null) {
                    currentPointedEntity.OnUnPointByCrosshair();
                    currentPointedEntity = null;
                }
            }
        }
    }
}
