using System;
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
        
        
        public override void OnStartOrAllocate() {
            base.OnStartOrAllocate();
            if (Pool) {
                transform.SetParent(null);
            }
        }

        public override void OnRecycled() {
            base.OnRecycled();
            transform.SetParent(Pool.transform);
        }

        
    }
}
