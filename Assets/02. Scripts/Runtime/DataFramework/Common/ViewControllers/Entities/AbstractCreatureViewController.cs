using BehaviorDesigner.Runtime;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Entities.Creatures;
using UnityEngine.AI;

namespace Runtime.DataFramework.ViewControllers.Entities {
	
	/// <summary>
	/// An abstract view controller for creature entity (like player, enemy, etc.)
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TEntityModel"></typeparam>
	public abstract class AbstractCreatureViewController<T> : AbstractDamagableViewController<T>
		where T : class, IHaveCustomProperties, IHaveTags, IDamageable, ICreature{
		NavMeshAgent navMeshAgent;
		BehaviorTree behaviorTree;
		protected override void Awake() {
			base.Awake();
			navMeshAgent = GetComponent<NavMeshAgent>();
			behaviorTree = GetComponent<BehaviorTree>();
			if (navMeshAgent) {
				navMeshAgent.enabled = false;
			}
			
			if (behaviorTree) {
				behaviorTree.enabled = false;
			}
			
		}

		protected override void OnEntityStart() {
			throw new System.NotImplementedException();
		}

		protected override void OnStart() {
			base.OnStart();
			if (navMeshAgent) {
				navMeshAgent.enabled = true;
			}
			
			if (behaviorTree) {
				behaviorTree.enabled = true;
				behaviorTree.Start();
			}
			
		}

		public override void OnRecycled() {
			base.OnRecycled();
			if (navMeshAgent) {
				navMeshAgent.enabled = false;
			}
			
			if (behaviorTree) {
				behaviorTree.enabled = false;
			}
			
		}
	}
}