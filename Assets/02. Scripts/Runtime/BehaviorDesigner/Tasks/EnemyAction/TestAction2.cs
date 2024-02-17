using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities.Collision;
using UnityEngine;

public class TestAction2 : Action {
	public float range;
	public SharedGameObjectList returnedHurtboxes;

	public override void OnStart() {
		Debug.Log("GetEnemyHurtboxesWithinRange OnStart");
		base.OnStart();
	}

	public override TaskStatus OnUpdate() {
		var hurtboxes = new List<GameObject>();
		var colliders = Physics.OverlapSphere(transform.position, range);

       
        
		foreach (var collider in colliders) {
			if (collider.GetComponent<IHurtbox>() != null || collider.GetComponent<HurtboxModifier>()) {
				if(collider.attachedRigidbody && collider.attachedRigidbody.TryGetComponent<IEnemyViewController>(out var enemyViewController)) {
					if(enemyViewController.EnemyEntity == null || enemyViewController.EnemyEntity.GetCurrentHealth() <= 0) {
						continue;
					}
                    
					hurtboxes.Add(collider.gameObject);
				}
			}
		}

		returnedHurtboxes.Value = hurtboxes;
		return TaskStatus.Success;
	}

	public override void OnEnd() {
		base.OnEnd();
	}
}
