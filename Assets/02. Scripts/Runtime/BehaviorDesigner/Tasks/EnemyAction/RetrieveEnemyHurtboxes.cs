using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities.Collision;
using UnityEngine;

public class RetrieveEnemyHurtboxes : Action
{
    public SharedFloat range;
    public SharedGameObjectList returnedHurtboxes;

    public override void OnStart() {
        Debug.Log("GetEnemyHurtboxesWithinRange OnStart");
        base.OnStart();
    }

    public override TaskStatus OnUpdate() {
        var hurtboxes = new List<GameObject>();
        var colliders = Physics.OverlapSphere(transform.position, range.Value);

       
        
        foreach (var collider in colliders) {
            if(collider.attachedRigidbody && collider.attachedRigidbody.TryGetComponent<IEnemyViewController>(out var enemyViewController)) {
                if(enemyViewController.EnemyEntity == null || enemyViewController.EnemyEntity.GetCurrentHealth() <= 0) {
                    continue;
                }

                if (collider.attachedRigidbody.CompareTag("Enemy")) {
                    hurtboxes.Add(collider.gameObject);
                }
            }
        }

        
        //sort hurtboxes by distance (closest to furthest)
        hurtboxes.Sort((x, y) => Vector3.Distance(transform.position, x.transform.position).
            CompareTo(Vector3.Distance(transform.position, y.transform.position)));


        returnedHurtboxes.Value = hurtboxes;
        return TaskStatus.Success;
    }

    public override void OnEnd() {
        base.OnEnd();
    }
}
