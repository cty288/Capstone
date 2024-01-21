using UnityEngine;

namespace _02._Scripts.Runtime.GameEventSystem.Tests {
	public class TestEvent1 : GameEvent<TestEvent1> {
		public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
		public override void OnInitialized() {
			
		}

		public override void OnTriggered() {
			Debug.Log("EVENT 1 TRIGGERED");
		}

		public override void OnLeaped() {
			Debug.Log("EVENT 1 ELAPSED");
		}

		public override bool CanPersistToOtherLevels { get; } = false;

		public override void OnEventRecycled() {
			
		}
	}
	
	public class TestEvent2 : GameEvent<TestEvent2> {
		public override EventElapseType ElapseType { get; } = EventElapseType.Predetermined;
		public override void OnInitialized() {
			
		}

		public override void OnTriggered() {
			Debug.Log("EVENT 2 TRIGGERED");
		}

		public override void OnLeaped() {
			Debug.Log("EVENT 2 ELAPSED");
		}

		public override bool CanPersistToOtherLevels { get; } = true;

		public override void OnEventRecycled() {
			
		}
	}
	
	public class TestEvent3 : GameEvent<TestEvent3> {
		public override EventElapseType ElapseType { get; } = EventElapseType.ExcludeTimeLeap;
		public override void OnInitialized() {
			
		}

		public override void OnTriggered() {
			Debug.Log("EVENT 3 TRIGGERED");
		}

		public override void OnLeaped() {
			Debug.Log("EVENT 3 ELAPSED");
		}

		public override bool CanPersistToOtherLevels { get; } = true;

		public override void OnEventRecycled() {
			
		}
	}
}