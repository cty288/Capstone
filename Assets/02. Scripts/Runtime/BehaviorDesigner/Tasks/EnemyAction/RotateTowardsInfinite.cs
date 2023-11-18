using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using a;
namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    [TaskDescription("Rotates towards the specified rotation. The rotation can either be specified by a transform or rotation. If the transform " +
                       "is used then the rotation will not be used.")]
    [TaskCategory("Movement")]
    
    [TaskIcon("04fb8138ea905c04ea39265f778fe1a4", "9bded0edc8b2a2f478fc28396fa41df2")]
    public class RotateTowardsInfinite : Action
    {
        
        [UnityEngine.Serialization.FormerlySerializedAs("usePhysics2D")]
        public bool m_UsePhysics2D;
       
        [UnityEngine.Serialization.FormerlySerializedAs("rotationEpsilon")]
        public SharedFloat m_RotationEpsilon = 0.5f;
       
        [UnityEngine.Serialization.FormerlySerializedAs("maxLookAtRotationDelta")]
        public SharedFloat m_MaxLookAtRotationDelta = 1;
       
        [UnityEngine.Serialization.FormerlySerializedAs("onlyY")]
        public SharedBool m_OnlyY;
       
        [UnityEngine.Serialization.FormerlySerializedAs("target")]
        public SharedGameObject m_Target;
        
        [UnityEngine.Serialization.FormerlySerializedAs("targetRotation")]
        public SharedVector3 m_TargetRotation;

        public override TaskStatus OnUpdate()
        {
            var rotation = Target();
            // Return a task status of success once we are done rotating
            if (Quaternion.Angle(transform.rotation, rotation) < m_RotationEpsilon.Value)
            {
                return TaskStatus.Running;
            }
            // We haven't reached the target yet so keep rotating towards it
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, m_MaxLookAtRotationDelta.Value);
           // Debug.Log("rotating");
            return TaskStatus.Running;
        }

        // Return targetPosition if targetTransform is null
        private Quaternion Target()
        {
            if (m_Target == null || m_Target.Value == null)
            {
                return Quaternion.Euler(m_TargetRotation.Value);
            }
            var position = m_Target.Value.transform.position - transform.position;
            if (m_OnlyY.Value)
            {
                position.y = 0;
            }
            if (m_UsePhysics2D)
            {
                var angle = Mathf.Atan2(position.y, position.x) * Mathf.Rad2Deg;
                return Quaternion.AngleAxis(angle, Vector3.forward);
            }
            return Quaternion.LookRotation(position);
        }

        // Reset the public variables
        public override void OnReset()
        {
            m_UsePhysics2D = false;
            m_RotationEpsilon = 0.5f;
            m_MaxLookAtRotationDelta = 1f;
            m_OnlyY = false;
            m_Target = null;
            m_TargetRotation = Vector3.zero;
        }
    }
}
