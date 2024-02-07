using MikroFramework.Architecture;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.DayNight
{
    
    public struct OnNightApproaching {
        public int RemainingMinutes;
    }
    public class NightWarningEvent : GameEvent<NightWarningEvent> {
        public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
        
        [ES3Serializable]
        private int _timeTilNight;
        
        public override void OnInitialized() {
        }

        public override void OnTriggered() {
            this.SendEvent<OnNightApproaching>(new OnNightApproaching()
            {
                RemainingMinutes = _timeTilNight
            });
        }

        public override void OnLeaped() {
			
        }

        public override bool CanPersistToOtherLevels { get; } = false;
        public override void OnEventRecycled() {
			
        }
        
        public static NightWarningEvent Allocate(int minutesTilNight) {
            NightWarningEvent nightWarningEvent = NightWarningEvent.Allocate();
            nightWarningEvent._timeTilNight = minutesTilNight;
            return nightWarningEvent;
        }
    }
}