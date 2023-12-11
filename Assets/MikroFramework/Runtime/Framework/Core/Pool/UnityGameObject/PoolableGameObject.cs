using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace MikroFramework.Pool
{
    public abstract class PoolableGameObject : MonoBehaviour
    {
        [SerializeField] private UnityEvent onRecycledEvent;
        [SerializeField] private UnityEvent onAllocateEvent;
        /// <summary>
        /// The pool that belongs to this object
        /// </summary>
        [HideInInspector]
        public GameObjectPool Pool;

        public virtual bool IsRecycled { get; set; } = false;
        
        private CancellationTokenSource cts 
            = new CancellationTokenSource();
        
        /// <summary>
        /// Call this method to recycle this gameobject back to its pool
        /// </summary>
        public void RecycleToCache()
        {
            if (!this) {
                return;
            }
            
            cts.Cancel();
            if (Pool != null) {
                Pool.Recycle(this.gameObject);
                Pool = null;
            }else {
                OnRecycled();
                Destroy(this.gameObject);
            }
          
        }

        private void Start() {
            if (Pool == null) {
                OnStartOrAllocate();
            }
        }

        public virtual void OnStartOrAllocate() {
            cts = new CancellationTokenSource();
            onAllocateEvent?.Invoke();
        }
        
        public CancellationToken GetCancellationTokenOnRecycle() {
            return cts.Token;
        }


        /// <summary>
        /// Triggered after recycled back to the pool, or after calling Recycle()
        /// </summary>
        public virtual void OnRecycled() {
            onRecycledEvent?.Invoke();
        }
        
        public void RegisterOnRecycledEvent(UnityAction action) {
            onRecycledEvent.AddListener(action);
        }
        
        public void RegisterOnAllocateEvent(UnityAction action) {
            onAllocateEvent.AddListener(action);
        }
        
        private void OnDestroy() {
            onRecycledEvent?.RemoveAllListeners();
            onAllocateEvent?.RemoveAllListeners();
        }
    }
}