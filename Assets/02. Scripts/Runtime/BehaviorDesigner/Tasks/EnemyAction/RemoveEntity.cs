using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.Entities;
using UnityEngine;

public class RemoveEntity : EnemyAction {
    public override void OnStart() {
        base.OnStart();
        GlobalEntities.GetEntityAndModel(enemyViewController.EnemyEntity.UUID).Item2
            .RemoveEntity(enemyViewController.EnemyEntity.UUID);
    }

    public override TaskStatus OnUpdate() {
        return TaskStatus.Success;
    }
}
