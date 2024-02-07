using MikroFramework.Architecture;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.DayNight
{
    
    public struct OnNightApproaching {
		
    }
    public class NightWarningEvent : GameEvent<NightWarningEvent> {
        public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
        public override void OnInitialized() {
        }

        public override void OnTriggered() {
            this.SendEvent<OnNightApproaching>();
        }

        public override void OnLeaped() {
			
        }

        public override bool CanPersistToOtherLevels { get; } = false;
        public override void OnEventRecycled() {
			
        }
    }
}