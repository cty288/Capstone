using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using DG.Tweening;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossMoveNearPlayer : EnemyAction
    {
        public SharedFloat speed;
        private TaskStatus taskStatus;
        private NavMeshAgent agent;
        private Rigidbody rb;
        
        private GameObject player;
        private Vector3 destination;
        private float startTme;
        public SharedFloat duration; // Duration in seconds
        private float sampleMinRadius = 40f;
        public SharedFloat sampleMaxRadius;

        
        public override void OnAwake()
        {
            agent = gameObject.GetComponent<NavMeshAgent>();
            rb = gameObject.GetComponent<Rigidbody>();
        }

        public override void OnStart()
        {
            // rb.isKinematic = false;
            player = GetPlayer();
            agent.enabled = true;
            agent.speed = speed.Value;
            
            Vector3 sample = MathFunctions.RandomPointInAnnulus(player.transform.position, sampleMinRadius, sampleMaxRadius.Value);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, 25, NavMesh.AllAreas))
            {
                destination = hit.position;
            }

            SkillExecute();
            
            taskStatus = TaskStatus.Running;
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        private async UniTask SkillExecute()
        {
            agent.enabled = true;
            
            startTme = Time.time;
            agent.SetDestination(destination);

            //wait for movement
            await UniTask.WaitUntil(() =>
                {
                    return ReachedDestination() || Time.time - startTme > duration.Value;
                },
                PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            taskStatus = TaskStatus.Success;
        }

        private bool ReachedDestination()
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance &&
                   (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }
    }
}
