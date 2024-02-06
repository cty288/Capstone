using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using MikroFramework.Pool;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;

namespace _02._Scripts.Runtime.Utilities.AsyncTriggerExtension {
	public static class AsyncTriggerExtension {
		public static CancellationToken GetCancellationTokenOnDestroyOrRecycleOrDie(this GameObject gameObject, bool cancelledOnStunned = true) {
			if(gameObject.TryGetComponent<ICreatureViewController>(out ICreatureViewController c)) {
				if (cancelledOnStunned) {
					return c.GetCancellationTokenOnStunnedOrDie();
				}
				else {
					return c.GetCancellationTokenOnDie();
				}
			
			}
			
			if (gameObject.TryGetComponent<IDamageableViewController>(out IDamageableViewController o)) {
				return o.GetCancellationTokenOnDie();
			}

			if (gameObject.TryGetComponent<PoolableGameObject>(out PoolableGameObject obj)) {
				 return obj.GetCancellationTokenOnRecycle();
			 }
			 return gameObject.GetCancellationTokenOnDestroy();
		 }
	}
}