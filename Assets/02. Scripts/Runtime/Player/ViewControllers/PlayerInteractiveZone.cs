using System.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Utilities;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;

namespace Runtime.Player {
    public class PlayerInteractiveZone : MonoBehaviour {
        private TriggerCheck interactiveHintTriggerCheck;
        
        [SerializeField]
        private float blockCheckInterval = 0.1f;
        private float blockCheckTimer = 0f;
        
        private Dictionary<GameObject, IEntityViewController> entityViewControllersNotBlocked = 
            new Dictionary<GameObject, IEntityViewController>();

        private Dictionary<GameObject, IEntityViewController> entityViewControllersBlocked =
            new Dictionary<GameObject, IEntityViewController>();

        private RaycastHit[] hits = new RaycastHit[5];
        
        
        [SerializeField]
        private LayerMask wallLayerMask;

        private void Awake() {
            interactiveHintTriggerCheck = GetComponent<TriggerCheck>();
            interactiveHintTriggerCheck.OnEnter += OnEnterInteractiveCheck;
            interactiveHintTriggerCheck.OnExit += OnExitInteractiveCheck;
            
        }

        private void OnExitInteractiveCheck(Collider other) {
            if (!other) {
                return;
            }
            
            Rigidbody rootRigidbody = other.attachedRigidbody;
            GameObject hitObj =
                rootRigidbody ? rootRigidbody.gameObject : other.gameObject;
            
            if(hitObj && hitObj.TryGetComponent<IEntityViewController>(out var entityViewController)) {
                if(entityViewControllersNotBlocked.ContainsKey(hitObj)) {
                    entityViewController.OnPlayerInteractiveZoneNotReachable(transform.parent.gameObject, this);
                }
                
                
                entityViewControllersNotBlocked.Remove(hitObj);
                entityViewControllersBlocked.Remove(hitObj);
               
               
                entityViewController.OnPlayerExitInteractiveZone(transform.parent.gameObject, this);
            }
        }

        private void OnEnterInteractiveCheck(Collider other) {
            Rigidbody rootRigidbody = other.attachedRigidbody;
            GameObject hitObj =
                rootRigidbody ? rootRigidbody.gameObject : other.gameObject;
            
            if(hitObj.TryGetComponent<IEntityViewController>(out var entityViewController)) {
                entityViewControllersBlocked.TryAdd(hitObj, entityViewController);
                //entityViewControllersInRange.Add(entityViewController);
                entityViewController.OnPlayerInInteractiveZone(transform.parent.gameObject, this);
                
            }
        }

        private void Update() {
            //check block status, check if blocked can be unblocked, or unblocked can be blocked. 
            //raycast from player to entity, if hit wall, then blocked, else unblocked
            blockCheckTimer += Time.deltaTime;
            if (blockCheckTimer >= blockCheckInterval) {
                blockCheckTimer = 0f;
                
                
                List<GameObject> removedVCs = new List<GameObject>();
                foreach (var entityVC in entityViewControllersBlocked) {
                    if (!entityVC.Key) {
                        continue;
                    }


                    var position = transform.position;
                    int numHits = Physics.RaycastNonAlloc(position, entityVC.Key.transform.position - position, hits, 100f);
                    var sortedHits = hits.OrderBy(hit => hit.distance).ToArray();
                    
                    bool hitWall = false;
                    bool hitTarget = false;
                    for (int i = 0; i < sortedHits.Length; i++) {
                        if (!sortedHits[i].collider || sortedHits[i].collider.isTrigger) {
                            continue;
                        }

                        Rigidbody rootRigidbody = sortedHits[i].collider.attachedRigidbody;
                        GameObject hitObj =
                            rootRigidbody ? rootRigidbody.gameObject : sortedHits[i].collider.gameObject;
                        
                        if (hitObj == gameObject) {
                            continue;
                        }
                        if (hitObj == entityVC.Key) {
                            entityViewControllersNotBlocked.TryAdd(entityVC.Key, entityVC.Value);
                           // entityViewControllersBlocked.Remove(entityVC.Key);
                            removedVCs.Add(entityVC.Key);
                            entityVC.Value?.OnPlayerInteractiveZoneReachable(transform.parent.gameObject, this);
                            hitTarget = true;
                            break;
                        }
                        
                        if (PhysicsUtility.IsInLayerMask(hitObj, wallLayerMask)) {
                            hitWall = true;
                            break;
                        }
                        
                    }
                    
                    if (!hitTarget && !hitWall) {
                        entityViewControllersNotBlocked.TryAdd(entityVC.Key, entityVC.Value);
                        removedVCs.Add(entityVC.Key);
                        entityVC.Value?.OnPlayerInteractiveZoneReachable(transform.parent.gameObject, this);
                    }
                }
                
                foreach (var removedVC in removedVCs) {
                    entityViewControllersBlocked.Remove(removedVC);
                }
                removedVCs.Clear();
                
                
                foreach (var entityVC in entityViewControllersNotBlocked) {
                    if (!entityVC.Key) {
                        continue;
                    }
                    
                    var position = transform.position;
                    int numHits = Physics.RaycastNonAlloc(position, entityVC.Key.transform.position - position, hits, 100f);
                    var sortedHits = hits.OrderBy(hit => hit.distance).ToArray();
                    
                    for (int i = 0; i < sortedHits.Length; i++) {
                        if (!sortedHits[i].collider) {
                            continue;
                        }
                        Rigidbody rootRigidbody = sortedHits[i].collider.attachedRigidbody;
                        GameObject hitObj =
                            rootRigidbody ? rootRigidbody.gameObject : sortedHits[i].collider.gameObject;
                        if (hitObj == gameObject) {
                            continue;
                        }
                        if (hitObj == entityVC.Key) {
                            break;
                        }
                        
                        if (PhysicsUtility.IsInLayerMask(hitObj, wallLayerMask)) {
                            entityViewControllersBlocked.TryAdd(entityVC.Key, entityVC.Value);
                            removedVCs.Add(entityVC.Key);
                            entityVC.Value.OnPlayerInteractiveZoneNotReachable(transform.parent.gameObject, this);
                            break;
                        }
                        
                    }
                }
                
                foreach (var removedVC in removedVCs) {
                    entityViewControllersNotBlocked.Remove(removedVC);
                }
            }
        }

        public bool IsInZone(GameObject entityViewController) {
            return entityViewControllersNotBlocked.ContainsKey(entityViewController);
        }

        private void OnDestroy() {
            interactiveHintTriggerCheck.OnEnter -= OnEnterInteractiveCheck;
            interactiveHintTriggerCheck.OnExit -= OnExitInteractiveCheck;
        }
    }
}
