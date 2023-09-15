using MikroFramework.Event;
using UnityEngine;

namespace Runtime.Utilities {
	public static class UnRegisterExtension {
		/// <summary>
		/// Unregister this listener when a specific gameObject is destroyed
		/// </summary>
		/// <param name="unRegister"></param>
		/// <param name="gameObject"></param>
		public static void UnRegisterWhenGameObjectDestroyedOrRecycled(this IUnRegister unRegister, GameObject gameObject, bool alsoUnRegisterWhenDisabled = false) {
			UnRegisterAllocateTrigger trigger = gameObject.GetComponent<UnRegisterAllocateTrigger>();

			if (!trigger) {
				trigger = gameObject.AddComponent<UnRegisterAllocateTrigger>();
			}

			trigger.AddUnRegister(unRegister, alsoUnRegisterWhenDisabled);
		}
	}
}