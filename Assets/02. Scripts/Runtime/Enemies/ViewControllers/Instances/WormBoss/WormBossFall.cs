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
using FIMSpace.FSpine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossFall : EnemyAction
    {
        public SharedVector3 previousDivePosition;
        private Vector3 emergePosition;
        public float minRadiusAroundPlayer = 40f;
        public float maxRadiusAroundPlayer = 70f;

        private GameObject player;

        private TaskStatus taskStatus;

        public TimelineAsset fallAnimationTimeline;
        private PlayableDirector director;

        private FSpineAnimator spineAnimator;

        public override void OnAwake()
        {
            spineAnimator = GetComponent<FSpineAnimator>();
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
                MathFunctions.RandomPointInAnnulus(player.transform.position, minRadiusAroundPlayer, maxRadiusAroundPlayer);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, 30, NavMeshHelper.GetSpawnableAreaMask()))
            {
                emergePosition = hit.position;
            }
            else
            {
                emergePosition = previousDivePosition.Value;
            }

            spineAnimator.GoBackSpeed = 1;

            float height = 30f;
            Vector3 targetPosition = emergePosition - new Vector3(0, height, 0);
            transform.position = targetPosition;
            
            await UniTask.WaitForSeconds(1f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            await PlayAnimation();
        }

    private async UniTask PlayAnimation()
        {
            //rotate y to face player
            Vector3 direction = player.transform.position - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction, transform.forward);
            var rotation = transform.rotation;
            rotation.eulerAngles = new Vector3(-90, lookRotation.eulerAngles.y, 0);
            transform.rotation = rotation;
            
            await UniTask.WaitForSeconds(1f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            director.Play(fallAnimationTimeline);
            await UniTask.WaitForSeconds((float)fallAnimationTimeline.duration, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            director.Stop();
            spineAnimator.GoBackSpeed = 0;
            await UniTask.WaitForSeconds(1f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            // float duration = 2f;
            // await transform.DOMove(emergePosition, duration).ToUniTask(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            taskStatus = TaskStatus.Success;
        }
   }
}
