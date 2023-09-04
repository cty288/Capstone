﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        /// <summary>
        /// Call this method to recycle this gameobject back to its pool
        /// </summary>
        public void RecycleToCache()
        {
            if (Pool != null) {
                Pool.Recycle(this.gameObject);
                Pool = null;
            }else {
                Destroy(this.gameObject);
            }
          
        }

        public virtual void OnAllocate() {
            onAllocateEvent?.Invoke();
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