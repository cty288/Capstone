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
using Runtime.Enemies.SmallEnemies;
using MikroFramework.AudioKit;
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
        private float extensionDistance = 5f;
        private bool moving;
        private Color startEmissionColor;
        public SharedGameObject e;
        private SafeGameObjectPool pool;
        // Start is called before the first frame update
        private float explosionTimer = 0.4f;
        public override void OnStart()
        {
            base.OnStart();
            agent = this.gameObject.GetComponent<NavMeshAgent>();
            agent.ResetPath();
            agent.speed = 9f;
            pool = GameObjectPoolManager.Singleton.CreatePool(e.Value, 20, 50);
        }

        // Update is called once per frame
        public override TaskStatus OnUpdate()
        {
            explosionTimer -= Time.deltaTime;
            if(explosionTimer < 0)
            {
                explosionTimer = 0.4f;
                AudioSource audio = AudioSystem.Singleton.Play3DSound("Drone_Explosion", this.gameObject.transform.position);
                audio.volume = 0.5f;
           
                GameObject explosion = pool.Allocate();
                explosion.transform.position = this.gameObject.transform.position;
                explosion.GetComponent<IExplosionViewController>().
                    Init(enemyEntity.CurrentFaction.Value, 
                        enemyEntity.GetCustomDataValue<int>("attack", "explosionDamage"),2, gameObject,
                    gameObject.GetComponent<ICanDealDamage>());
            }
            if (player.Value != null && moving == false) // Check if the player reference is valid
            {
                Debug.Log("rolling");
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
            explosionTimer = 0.4f;
            
        }
    }

    
}