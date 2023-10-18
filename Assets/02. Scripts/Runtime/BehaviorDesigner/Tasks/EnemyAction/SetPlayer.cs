using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction {
	public class SetPlayer : EnemyAction {
		public SharedGameObject playerToSet;
		
		public override void OnStart() {
			base.OnStart();
			playerToSet.Value = GetPlayer();
			Debug.Log(playerToSet.Value);
		}
		
		public override TaskStatus OnUpdate() {
			return TaskStatus.Success;
		}
	}
}