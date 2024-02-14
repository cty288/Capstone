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
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossPrepareEmerge : EnemyAction
    {
        public SharedVector3 previousDivePosition;
        private Vector3 emergePosition;
        public float minRadiusAroundPlayer = 40f;
        public float maxRadiusAroundPlayer = 70f;
        
        BoxCollider spawnsizeCollider() => gameObject.GetComponent<ICreatureViewController>().SpawnSizeCollider;
        private bool finishCalculatePath = false;
        
        private GameObject player;

        public override void OnStart()
        {
            player = GetPlayer();
            
            transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            
            // float height = spawnsizeCollider().bounds.size.y;
            float height = 15f;
            Vector3 targetPosition = previousDivePosition.Value - new Vector3(0, height, 0);
            transform.position = targetPosition;
            
            GameObject debug_obj = new GameObject("worm emerge ready position");
            debug_obj.transform.position = targetPosition;

            ReorientFaceUpwards();
            // Vector3 sample = RandomPointInAnnulus(player.transform.position, minRadiusAroundPlayer, maxRadiusAroundPlayer);
            //
            // NavMeshHit hit;
            // if (NavMesh.SamplePosition(sample, out hit, 15, 1))
            // {
            //     emergePosition = hit.position;
            // }
        }

        private async UniTask ReorientFaceUpwards()
        {
            transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            await UniTask.Delay(1000);
        }

        private Vector3 RandomPointInAnnulus(Vector2 origin, float minRadius, float maxRadius){
            var randomDirection = (Random.insideUnitCircle * origin).normalized;
            var randomDistance = Random.Range(minRadius, maxRadius);
            var point = origin + randomDirection * randomDistance;
     
            return new Vector3(point.x, 0, point.y);
        }
    }
}
