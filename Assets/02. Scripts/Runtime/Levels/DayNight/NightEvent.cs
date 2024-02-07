using _02._Scripts.Runtime.Levels.Sandstorm;
using MikroFramework.Architecture;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.DayNight
{
    public struct OnNightStart {
		
    }
    public class NightEvent: GameEvent<NightEvent> {
        public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
        public override void OnInitialized() {
            Debug.Log($"Night approaches in {RemainingMinutesToTrigger} minutes!");
        }

        public override void OnTriggered() {
            this.SendEvent<OnNightStart>();
        }

        public override void OnLeaped() {
			
        }

        public override bool CanPersistToOtherLevels { get; } = false;
        public override void OnEventRecycled() {
			
        }
    }
}