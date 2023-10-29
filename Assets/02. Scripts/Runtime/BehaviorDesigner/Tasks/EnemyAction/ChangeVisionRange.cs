using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks.Movement;
using BehaviorDesigner.Runtime.Tasks;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class ChangeVisionRange : EnemyAction
    {
        public WithinDistance wd;
        public float range;
        // Start is called before the first frame update
        public override void OnStart()
        {
            base.OnStart();
            wd.m_Magnitude = range;
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
}
