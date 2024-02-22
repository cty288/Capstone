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
using MikroFramework.ActionKit;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossDive : EnemyAction
    {
        private TaskStatus taskStatus;
        private NavMeshAgent agent;
        private Rigidbody rb;
        
        public TimelineAsset diveAnimationTimeline;
        private PlayableDirector director;
        
        public override void OnAwake()
        {
            agent = gameObject.GetComponent<NavMeshAgent>();
            rb = gameObject.GetComponent<Rigidbody>();
            director = gameObject.GetComponent<PlayableDirector>();
        }

        public override void OnStart()
        {
            agent.enabled = false;
            

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
            director.Play(diveAnimationTimeline);
            await UniTask.WaitForSeconds((float)diveAnimationTimeline.duration, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            director.Stop();
            
            // Vector3 dir = new Vector3(90, 0, 0);
            // float duration = (transform.rotation.eulerAngles - dir).magnitude / 45f; // every 30 degree takes 1 second 
            // await transform.DORotate(dir, duration)
            //     .WithCancellation(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            //wait for movement
            // Vector3 endPos = transform.position + transform.forward * 150f;
            // duration = (transform.position - endPos).magnitude / 10f; // every 50 units takes 1 second
            // await transform.DOMove(endPos, duration)
            //     .WithCancellation(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            taskStatus = TaskStatus.Success;
        }
    }
}
