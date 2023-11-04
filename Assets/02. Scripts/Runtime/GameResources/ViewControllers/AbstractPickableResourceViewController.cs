using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Utilities;
using UnityEngine;
using Sequence = MikroFramework.ActionKit.Sequence;

namespace Runtime.GameResources.ViewControllers {
    public interface IPickableResourceViewController : IResourceViewController {
        public bool HoldAbsorb { get; set; }
        
        public IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity);
    }

    /// <summary>
    /// For all resources on the ground
    /// </summary>
//[RequireComponent(typeof(TriggerCheck))]
//[RequireComponent(typeof(PoolableGameObject))]
    public abstract class AbstractPickableResourceViewController<T> : AbstractResourceViewController<T>,
        IPickableResourceViewController where T : class, IResourceEntity, new() {
    
       // protected ResLoader resLoader;
        //protected PoolableGameObject poolable;
        protected IInventoryModel inventoryModel;
        protected bool isAbsorbing = false;
        protected bool isAbsorbWaiting = false;
        protected Dictionary<Collider, bool> selfColliders = new Dictionary<Collider, bool>();
        
        [Header("Entity Recycle Logic")]
        [Tooltip("The time when the entity will be recycled when it is not absorbed by the player")]
        [SerializeField] [ES3Serializable]
        protected float entityAutoRemovalTimeWhenNoAbsorb = 120f;
        
        private Coroutine entityRemovalTimerCoroutine;
        protected override void Awake() {
            base.Awake();
            //poolable = GetComponent<PoolableGameObject>();
           // resLoader = this.GetUtility<ResLoader>();DescriptionTag
            inventoryModel = this.GetModel<IInventoryModel>();
            foreach (Collider selfCollider in GetComponentsInChildren<Collider>(true)) {
                selfColliders.Add(selfCollider, selfCollider.isTrigger);
            }
        }
        
        

        protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = true;

        protected override void OnEntityStart() {
            if (entityAutoRemovalTimeWhenNoAbsorb > 0) {
                entityRemovalTimerCoroutine = StartCoroutine(EntityRemovalTimer());
            }
        }

        public override void OnPlayerInteractiveZoneReachable(GameObject player, PlayerInteractiveZone zone) {
            base.OnPlayerInteractiveZoneReachable(player, zone);
            if (!HoldAbsorb) {
                HandleAbsorb(player, zone);
            }
        }

        public override void OnPlayerInteractiveZoneNotReachable(GameObject player, PlayerInteractiveZone zone) {
            base.OnPlayerInteractiveZoneNotReachable(player, zone);
           
        }

        public override void OnPlayerExitInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
            base.OnPlayerExitInteractiveZone(player, zone);
            HoldAbsorb = false;
        }

        protected virtual void HandleAbsorb(GameObject player, PlayerInteractiveZone zone) {
            if (!player || !Camera.main || isAbsorbing || isAbsorbWaiting) return;
            StartCoroutine(AbsorbWait(player, zone));
        }

        private IEnumerator AbsorbWait(GameObject player, PlayerInteractiveZone zone) {
            isAbsorbWaiting = true;
            yield return new WaitForSeconds(0.2f);
            isAbsorbWaiting = false;
            if(AbsorbHandler(BoundEntity)) {
                isAbsorbing = true;
                if (entityRemovalTimerCoroutine != null) {
                    StopCoroutine(entityRemovalTimerCoroutine);
                    entityRemovalTimerCoroutine = null;
                }
                foreach (Collider selfCollider in selfColliders.Keys) {
                    selfCollider.isTrigger = true;
                }
                OnStartAbsorb();
                transform.DOMoveInTargetLocalSpace(Camera.main.transform, Vector3.zero, 0.2f).
                    SetEase(Ease.Linear)
                    .OnComplete(RecycleToCache);
            }
            else {
                //did not add successfully, will wait until the player has enough space, use Until Action
                //TODO: test it
                Sequence.Allocate()
                    .AddAction(
                        UntilAction.Allocate(() => inventoryModel.CanPlaceItem(BoundEntity) || zone.IsInZone(gameObject) || !this))
                    .AddAction(CallbackAction.Allocate(() => {
                        if (this && zone.IsInZone(gameObject)) {
                            HandleAbsorb(player, zone);
                        }
                    })).Execute();
            }
        }
        
        /// <summary>
        /// Handle the absorb logic, return true if the item is successfully absorbed
        /// By default, it will add the item to the inventory
        /// But you can override it to have your own logic instead of adding it to the inventory
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool AbsorbHandler(T entity) {
            return inventoryModel.AddItem(entity);
        }

        protected abstract void OnStartAbsorb();

        public override void OnRecycled() {
            base.OnRecycled();
           
            
        }

        protected override void OnReadyToRecycle() {
            base.OnReadyToRecycle();
            isAbsorbing = false;
            isAbsorbWaiting = false;
            if (entityRemovalTimerCoroutine != null) {
                StopCoroutine(entityRemovalTimerCoroutine);
                entityRemovalTimerCoroutine = null;
            }
            foreach (Collider selfCollider in selfColliders.Keys) {
                selfCollider.isTrigger = selfColliders[selfCollider];
            }
        }

        private IEnumerator EntityRemovalTimer() {
            yield return new WaitForSeconds(entityAutoRemovalTimeWhenNoAbsorb);
            if (this && entityModel!=null && entityAutoRemovalTimeWhenNoAbsorb >= 0) {
                entityModel.RemoveEntity(BoundEntity.UUID);
            }
        }

        /// <summary>
        /// Hold absorb when the item is just thrown
        /// </summary>
        public bool HoldAbsorb { get; set; }

        public abstract IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity);

        protected override IEntity OnBuildNewEntity() {
            return OnBuildNewPickableResourceEntity(false, 1);
        }
    }
}