using System.Collections;
using DG.Tweening;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Utilities;
using UnityEngine;
using Sequence = MikroFramework.ActionKit.Sequence;

namespace Runtime.GameResources.ViewControllers {
    public interface IPickableResourceViewController : IResourceViewController {
        public bool HoldAbsorb { get; set; }
    }

    /// <summary>
    /// For all resources on the ground
    /// </summary>
//[RequireComponent(typeof(TriggerCheck))]
//[RequireComponent(typeof(PoolableGameObject))]
    public abstract class AbstractPickableResourceViewController<T> : AbstractResourceViewController<T>,
        IPickableResourceViewController where T : class, IResourceEntity, new() {
    
        protected ResLoader resLoader;
        //protected PoolableGameObject poolable;
        protected IInventoryModel inventoryModel;
        protected bool isAbsorbing = false;
        
        [Header("Entity Recycle Logic")]
        [Tooltip("The time when the entity will be recycled when it is not absorbed by the player")]
        [SerializeField] protected float entityAutoRemovalTimeWhenNoAbsorb = 120f;
        
        private Coroutine entityRemovalTimerCoroutine;
        protected override void Awake() {
            base.Awake();
            //poolable = GetComponent<PoolableGameObject>();
            resLoader = this.GetUtility<ResLoader>();
            inventoryModel = this.GetModel<IInventoryModel>();
        }

        protected override void OnEntityStart() {
            if (entityAutoRemovalTimeWhenNoAbsorb >= 0) {
                entityRemovalTimerCoroutine = StartCoroutine(EntityRemovalTimer());
            }
        }

        public override void OnPlayerEnterInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
            base.OnPlayerEnterInteractiveZone(player, zone);
            if (!HoldAbsorb) {
                HandleAbsorb(player, zone);
            }
        }

        public override void OnPlayerExitInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
            base.OnPlayerExitInteractiveZone(player, zone);
            HoldAbsorb = false;
        }

        private void HandleAbsorb(GameObject player, PlayerInteractiveZone zone) {
            if (!player || !Camera.main || isAbsorbing) return;
            if(inventoryModel.AddItem(BoundEntity)) {
                isAbsorbing = true;
                if (entityRemovalTimerCoroutine != null) {
                    StopCoroutine(entityRemovalTimerCoroutine);
                    entityRemovalTimerCoroutine = null;
                }
                OnStartAbsorb();
                transform.DOMoveInTargetLocalSpace(Camera.main.transform, Vector3.zero, 0.2f).
                    SetEase(Ease.Linear)
                    .OnComplete(() => {
                        Destroy(this.gameObject);
                    });
            }
            else {
                //did not add successfully, will wait until the player has enough space, use Until Action
                //TODO: test it
                Sequence.Allocate()
                    .AddAction(
                        UntilAction.Allocate(() => inventoryModel.CanPlaceItem(BoundEntity) || zone.IsInZone(this) || !this))
                    .AddAction(CallbackAction.Allocate(() => {
                        if (this && zone.IsInZone(this)) {
                            HandleAbsorb(player, zone);
                        }
                    })).Execute();
            }
        }

        protected abstract void OnStartAbsorb();

        public override void OnRecycled() {
            base.OnRecycled();
            isAbsorbing = false;
            if (entityRemovalTimerCoroutine != null) {
                StopCoroutine(entityRemovalTimerCoroutine);
                entityRemovalTimerCoroutine = null;
            }
        }
        
        private IEnumerator EntityRemovalTimer() {
            yield return new WaitForSeconds(entityAutoRemovalTimeWhenNoAbsorb);
            if (this && entityModel!=null) {
                entityModel.RemoveEntity(BoundEntity.UUID);
            }
        }

        /// <summary>
        /// Hold absorb when the item is just thrown
        /// </summary>
        public bool HoldAbsorb { get; set; }
    }
}