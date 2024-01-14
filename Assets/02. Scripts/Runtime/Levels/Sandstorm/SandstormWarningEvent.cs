using MikroFramework.Architecture;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.Sandstorm {
	public struct OnSandStormWarning {
		public int RemainingMinutes;
	}



	public class SandstormWarningEvent : GameEvent<SandstormWarningEvent> {
		public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
		private int timeToTrigger = 0;
		public override void OnInitialized() {
			timeToTrigger = RemainingMinutesToTrigger;
		}

		public override void OnTriggered() {
			this.SendEvent<OnSandStormWarning>(new OnSandStormWarning() {
				RemainingMinutes = timeToTrigger
			});
		}

		public override void OnLeaped() {
			
		}

		public override bool CanPersistToOtherLevels { get; } = false;
		public override void OnEventRecycled() {
			timeToTrigger = 0;
		}
	}
}