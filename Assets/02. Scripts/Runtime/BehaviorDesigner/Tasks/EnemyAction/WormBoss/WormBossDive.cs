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
using Runtime.Enemies;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossDive : EnemyAction<WormBossEntity>
    {
        private TaskStatus taskStatus;
        private NavMeshAgent agent;
        private Rigidbody rb;
        
        public TimelineAsset diveAnimationTimeline;
        private PlayableDirector director;
        
        public override void OnAwake()
        {
            base.OnAwake();
            agent = gameObject.GetComponent<NavMeshAgent>();
            rb = gameObject.GetComponent<Rigidbody>();
            director = gameObject.GetComponent<PlayableDirector>();
        }

        public override void OnStart()
        {
            base.OnStart();
            taskStatus = TaskStatus.Running;

            agent.enabled = false;

            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        private async UniTask SkillExecute()
        {
            enemyEntity.isUnderground = true;
            enemyEntity.lastDivePosition = transform.position;
            
            director.Play(diveAnimationTimeline);
            await UniTask.WaitForSeconds((float)diveAnimationTimeline.duration, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            director.Stop();
            
            taskStatus = TaskStatus.Success;
        }
    }
}
