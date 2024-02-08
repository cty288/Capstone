﻿using MikroFramework.Architecture;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.DayNight
{
    public struct OnNewDay {
		
    }
    public class NewDayEvent: GameEvent<NewDayEvent> {
        public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
        public override void OnInitialized() {
            Debug.Log($"Day approaches in {RemainingMinutesToTrigger} minutes!");
        }

        public override void OnTriggered() {
            this.SendEvent<OnNewDay>();
            Debug.LogError("Dawn!");
        }

        public override void OnLeaped() {
        }

        public override bool CanPersistToOtherLevels { get; } = false;
        public override void OnEventRecycled() {
            Debug.LogError("Darn!");
        }
    }
}