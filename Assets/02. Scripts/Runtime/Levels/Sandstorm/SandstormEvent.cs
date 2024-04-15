using MikroFramework.Architecture;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Sandstorm {

	public struct OnSandStormKillPlayer {
		
	}
	
	public class SandstormEvent : GameEvent<SandstormEvent> {
		[field: ES3Serializable]
		public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
		public override void OnInitialized() {
			Debug.Log($"A sandstorm is coming in {RemainingMinutesToTrigger} minutes!");
		
		}

		public override void OnTriggered() {
			this.SendEvent<OnSandStormKillPlayer>();
		}

		public override void OnLeaped() {
			
		}

		[field: ES3Serializable]
		public override bool CanPersistToOtherLevels { get; } = false;
		public override void OnEventRecycled() {
			
		}
	}
}