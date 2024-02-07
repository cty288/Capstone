using MikroFramework.Architecture;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.DayNight
{
    public struct OnNewDay {
		
    }
    public class NewDayEvent: GameEvent<NewDayEvent> {
        public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
        public override void OnInitialized() {
            Debug.Log($"Night approaches in {RemainingMinutesToTrigger} minutes!");
        }

        public override void OnTriggered() {
            this.SendEvent<OnNewDay>();
        }

        public override void OnLeaped() {
			
        }

        public override bool CanPersistToOtherLevels { get; } = false;
        public override void OnEventRecycled() {
			
        }
    }
}