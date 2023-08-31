﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikroFramework.Pool;
using UnityEngine;
using UnityEngine.Events;

namespace MikroFramework.ResKit
{
    public class DefaultPoolableGameObject: PoolableGameObject {
        [SerializeField] private UnityEvent onRecycledEvent;
        [SerializeField] private UnityEvent onAllocateEvent;
        
        public override void OnAllocate() {
            onAllocateEvent?.Invoke();
            transform.SetParent(null);
        }

        public override void OnRecycled() {
            onRecycledEvent?.Invoke();
            transform.SetParent(Pool.transform);
        }
    }
}
