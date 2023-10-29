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
using Runtime.Enemies.SmallEnemies;
using UnityEngine.AI;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WheelRoll : EnemyAction<SpineWheelEntity>
    {
        public Material mat;
        public float intensity;
        public Color targetEmissionColor = new Color(191, 1, 40); // Set the target emission color in the Inspector.
        public float lerpSpeed = 3.0f; // Set the lerping speed in seconds.
        public float timer = 3f;
        public SharedGameObject player; // Reference to the player GameObject
        private NavMeshAgent agent;
        private float extensionDistance;
        private bool moving;
        private Color startEmissionColor;

        // Start is called before the first frame update
        public override void OnStart()
        {
            
            agent = this.gameObject.GetComponent<NavMeshAgent>();
            agent.ResetPath();
            agent.speed = 9f;
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            if (player != null && moving == false) // Check if the player reference is valid
            {
                // Calculate a vector from the enemy to the player
                Vector3 toPlayer = player.Value.transform.position - transform.position;

                // Calculate the target position as an extension of the line toward the player
                Vector3 targetPosition = player.Value.transform.position + toPlayer.normalized * extensionDistance;

                // Move the enemy to the target position using NavMesh
                agent.SetDestination(targetPosition);

                moving = true;
            }
            if(agent.remainingDistance < 0.1f)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Running;
            }
        }
        public override void OnEnd()
        {
            moving = false;
            agent.speed = 2f;
        }
    }

    
}