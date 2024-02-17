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
    public class WormBossMoveForwards : EnemyAction
    {
        private TaskStatus taskStatus;
        private NavMeshAgent agent;
        private Rigidbody rb;
        
        private GameObject player;
        private Vector3 destination;
        private float startTme;
        public float duration = 5f; // Duration in seconds
        
        public override void OnAwake()
        {
            agent = gameObject.GetComponent<NavMeshAgent>();
            rb = gameObject.GetComponent<Rigidbody>();
        }

        public override void OnStart()
        {
            // rb.isKinematic = false;
            player = GetPlayer();
            
            Vector3 sample = RandomPointInAnnulus(player.transform.position, 20, 40);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, 10, NavMesh.AllAreas))
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
            // wait for turn head
            Vector3 dir = (destination - transform.position).normalized;

            float rotDuration = (transform.rotation.eulerAngles - dir).magnitude / 45f; // every 90 degree takes 1 second 
            await transform.DORotate(dir, rotDuration)
                .WithCancellation(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            agent.enabled = true;
            
            startTme = Time.time;
            agent.SetDestination(destination);

            //wait for movement
            await UniTask.WaitUntil(() =>
                {
                    return ReachedDestination() || Time.time - startTme > duration;
                },
                PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            taskStatus = TaskStatus.Success;
        }

        private bool ReachedDestination()
        {
            return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance &&
                   (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }

        private Vector3 RandomPointInAnnulus(Vector2 origin, float minRadius, float maxRadius){
            var randomDirection = (Random.insideUnitCircle * origin).normalized;
            var randomDistance = Random.Range(minRadius, maxRadius);
            var point = origin + randomDirection * randomDistance;
     
            return new Vector3(point.x, 0, point.y);
        }
    }
}
