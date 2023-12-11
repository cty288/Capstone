using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using MikroFramework.Pool;
using UnityEngine;

namespace _02._Scripts.Runtime.Utilities.AsyncTriggerExtension {
	public static class AsyncTriggerExtension {
		public static CancellationToken GetCancellationTokenOnDestroyOrRecycle(this GameObject gameObject) {
			 if (gameObject.TryGetComponent<PoolableGameObject>(out PoolableGameObject obj)) {
				 return obj.GetCancellationTokenOnRecycle();
			 }
			 return gameObject.GetCancellationTokenOnDestroy();
		 }
	}
}