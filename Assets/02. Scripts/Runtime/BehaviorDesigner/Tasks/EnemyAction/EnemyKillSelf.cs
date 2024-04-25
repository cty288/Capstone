using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using UnityEngine;

public class EnemyKillSelf : EnemyAction
{
    public override void OnStart() {
        base.OnStart();
        enemyViewController.EnemyEntity.Kill(enemyViewController.EnemyEntity);
    }

    public override TaskStatus OnUpdate() {
        return TaskStatus.Success;
    }
}
