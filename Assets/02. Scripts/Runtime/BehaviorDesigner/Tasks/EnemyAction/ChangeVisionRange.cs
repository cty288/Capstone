using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks.Movement;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.Enemies.Model;
using Runtime.Enemies.SmallEnemies;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ChangeVisionRange : EnemyAction
    {
        public WithinDistance wd;
        public float range;
        public bool isGoingInRange;
        // Start is called before the first frame update
        public override void OnStart()
        {
            base.OnStart();
            IEnemyEntity entity = enemyViewController.EnemyEntity;
            if (isGoingInRange) {
                
                range = entity.GetCustomDataValue<float>("attack", "rangeAfterInteraction");
            }
            else
            {
                range = entity.GetCustomDataValue<float>("attack", "softDetectionRange");
            }
            
            wd.m_Magnitude = range;
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
