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
using UnityEngine.Playables;
using UnityEngine.Timeline;
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

        public TimelineAsset emergeAnimationTimeline;
        private PlayableDirector director;

        public override void OnAwake()
        {
            director = gameObject.GetComponent<PlayableDirector>();
        }

        public override void OnStart()
        {
            player = GetPlayer();

            //face upwards
            Debug.Log("WORM BOSS ROTATE");
            transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
            taskStatus = TaskStatus.Running;

            SkillExecute();
        }

        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        private async UniTask SkillExecute()
        {
            Vector3 sample =
                RandomPointInAnnulus(player.transform.position, minRadiusAroundPlayer, maxRadiusAroundPlayer);

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
            
            await UniTask.WaitForSeconds(1f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            await SlitherUp();
        }

    private async UniTask SlitherUp()
        {
            //rotate y to face player
            // Vector3 direction = player.transform.position - transform.position;
            // transform.rotation = Quaternion.LookRotation(direction, transform.forward);
            
            director.Play(emergeAnimationTimeline);
            await UniTask.WaitForSeconds((float)emergeAnimationTimeline.duration, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            director.Stop();
            await UniTask.WaitForSeconds(1f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            // float duration = 2f;
            // await transform.DOMove(emergePosition, duration).ToUniTask(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
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
