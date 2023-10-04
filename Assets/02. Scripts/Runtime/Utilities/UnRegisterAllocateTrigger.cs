using System;
using MikroFramework.Event;
using MikroFramework.Pool;
using UnityEngine;

namespace Runtime.Utilities {
	public class UnRegisterAllocateTrigger : UnRegisterDestroyTrigger {
		private PoolableGameObject poolableGameObject;

		private void Awake() {
			poolableGameObject = GetComponent<PoolableGameObject>();
			if (!poolableGameObject) {
				poolableGameObject = GetComponentInParent<PoolableGameObject>();
			}
			
			if (poolableGameObject) {
				poolableGameObject.RegisterOnRecycledEvent(OnRecycled);
			}
		}

		private void OnRecycled() {
			OnDestroy();
		}
	}
}