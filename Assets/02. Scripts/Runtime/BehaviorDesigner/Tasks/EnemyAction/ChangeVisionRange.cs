using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks.Movement;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.Enemies.SmallEnemies;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ChangeVisionRange : EnemyAction<SpineWheelEntity>
    {
        public WithinDistance wd;
        public float range;
        public bool isGoingInRange;
        // Start is called before the first frame update
        public override void OnStart()
        {
            base.OnStart();
            if (isGoingInRange)
            {
                range = enemyEntity.GetCustomDataValue<float>("attack", "rangeAfterInteraction");
            }
            else
            {
                range = enemyEntity.GetCustomDataValue<float>("attack", "softDetectionRange");
            }
            
            wd.m_Magnitude = range;
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
