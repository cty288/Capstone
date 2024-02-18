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
    public class WormBossEmerge : EnemyAction
    {
        public SharedVector3 previousDivePosition;
        private Vector3 emergePosition;
        public float minRadiusAroundPlayer = 40f;
        public float maxRadiusAroundPlayer = 70f;
        
        private GameObject player;
        
        private TaskStatus taskStatus;

        public override void OnStart()
        {
            player = GetPlayer();
            
            //face upwards
            transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            
            Vector3 sample = RandomPointInAnnulus(player.transform.position, minRadiusAroundPlayer, maxRadiusAroundPlayer);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, 30, NavMeshHelper.GetSpawnableAreaMask()))
            {
                emergePosition = hit.position;
            }
            else
            {
                emergePosition = previousDivePosition.Value;
            }
            
            float height = 30f;
            Vector3 targetPosition = emergePosition - new Vector3(0, height, 0);
            transform.position = targetPosition;
            
            taskStatus = TaskStatus.Running;
            SlitherUp();
        }

        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        private async UniTask SlitherUp()
        {
            float duration = 2f;
            await transform.DOMove(emergePosition, duration).ToUniTask(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            taskStatus = TaskStatus.Success;
        }

        private Vector3 RandomPointInAnnulus(Vector2 origin, float minRadius, float maxRadius){
            var randomDirection = (Random.insideUnitCircle * origin).normalized;
            var randomDistance = Random.Range(minRadius, maxRadius);
            var point = origin + randomDirection * randomDistance;
     
            return new Vector3(point.x, 0, point.y);
        }
    }
}
