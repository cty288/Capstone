using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using Cinemachine;
using BehaviorDesigner.Runtime.Tasks.Movement;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    
        [TaskDescription("Patrol around the specified waypoints using the Unity NavMesh.")]
        [TaskCategory("Movement")]
        
        [TaskIcon("9db06eafffd691549994cfe903905580", "3c16815a0806b2a4c8cd693c5139b3ea")]
        public class DronePatrol : NavMeshMovement
        {
         
            [UnityEngine.Serialization.FormerlySerializedAs("randomPatrol")]
            public SharedBool m_RandomPatrol;
       
            [UnityEngine.Serialization.FormerlySerializedAs("waypointPauseDuration")]
            public SharedFloat m_WaypointPauseDuration;
           
            [UnityEngine.Serialization.FormerlySerializedAs("waypoints")]
            public SharedGameObjectList m_Waypoints;

            // The current index that we are heading towards within the waypoints array
            private int m_WaypointIndex;
            private float m_WaypointReachedTime;
            public float range;

            public override void OnStart()
            {
                Vector3 referencePoint = this.gameObject.transform.position;
                Vector2 randomOffset1 = Random.insideUnitCircle * range;
                Vector2 randomOffset2 = Random.insideUnitCircle * range;
                m_Waypoints.Value[0].transform.position = new Vector3(referencePoint.x + randomOffset1.x, referencePoint.y, referencePoint.z + randomOffset1.y);
                m_Waypoints.Value[1].transform.position = new Vector3(referencePoint.x + randomOffset2.x, referencePoint.y, referencePoint.z + randomOffset2.y);




            base.OnStart();

                // initially move towards the closest waypoint
                float distance = Mathf.Infinity;
                float localDistance;
                for (int i = 0; i < m_Waypoints.Value.Count; ++i)
                {
                    if ((localDistance = Vector3.Magnitude(transform.position - m_Waypoints.Value[i].transform.position)) < distance)
                    {
                        distance = localDistance;
                        m_WaypointIndex = i;
                    }
                }
                m_WaypointReachedTime = -1;
                SetDestination(Target());
            }

            // Patrol around the different waypoints specified in the waypoint array. Always return a task status of running. 
            public override TaskStatus OnUpdate()
            {
            //Debug.Log("runninini");
                if (m_Waypoints.Value.Count == 0)
                {
                    return TaskStatus.Failure;
                }
                if (HasArrived())
                {
                    if (m_WaypointReachedTime == -1)
                    {
                        m_WaypointReachedTime = Time.time;
                    }
                    // wait the required duration before switching waypoints.
                    if (m_WaypointReachedTime + m_WaypointPauseDuration.Value <= Time.time)
                    {
                        if (m_RandomPatrol.Value)
                        {
                            if (m_Waypoints.Value.Count == 1)
                            {
                                m_WaypointIndex = 0;
                            }
                            else
                            {
                                // prevent the same waypoint from being selected
                                var newWaypointIndex = m_WaypointIndex;
                                while (newWaypointIndex == m_WaypointIndex)
                                {
                                    newWaypointIndex = Random.Range(0, m_Waypoints.Value.Count);
                                }
                                m_WaypointIndex = newWaypointIndex;
                            }
                        }
                        else
                        {
                            m_WaypointIndex = (m_WaypointIndex + 1) % m_Waypoints.Value.Count;
                        }
                        SetDestination(Target());
                        m_WaypointReachedTime = -1;
                    }
                }

                return TaskStatus.Running;
            }

            // Return the current waypoint index position
            private Vector3 Target()
            {
                if (m_WaypointIndex >= m_Waypoints.Value.Count)
                {
                    return transform.position;
                }
                return m_Waypoints.Value[m_WaypointIndex].transform.position;
            }

            // Reset the public variables
            public override void OnReset()
            {
                base.OnReset();

                m_RandomPatrol = false;
                m_WaypointPauseDuration = 0;
                m_Waypoints = null;
            }

            // Draw a gizmo indicating a patrol 
            public override void OnDrawGizmos()
            {
#if UNITY_EDITOR
                if (m_Waypoints == null || m_Waypoints.Value == null)
                {
                    return;
                }
                var oldColor = UnityEditor.Handles.color;
                UnityEditor.Handles.color = Color.yellow;
                for (int i = 0; i < m_Waypoints.Value.Count; ++i)
                {
                    if (m_Waypoints.Value[i] != null)
                    {
                        UnityEditor.Handles.SphereHandleCap(0, m_Waypoints.Value[i].transform.position, m_Waypoints.Value[i].transform.rotation, 1, EventType.Repaint);
                    }
                }
                UnityEditor.Handles.color = oldColor;
#endif
            }
        }
    
}