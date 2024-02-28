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
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using DG.Tweening;
using FIMSpace.FSpine;
using MikroFramework;
using MoreMountains.Feedbacks;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class WormBossEndAim : EnemyAction
    {
        public SharedVector3 _originPos;
        public SharedFloat liftTime;
        
        private int neckJointIndex = 0;
        private int bodyJointIndex = 16;
        
        public SharedGameObject neckSupportSphere;
        public SharedGameObject bodySupportPlatform;

        private Transform supportSphere;
        private Transform supportPlatform;
        
        private FSpineAnimator _spine;
        
        private NavMeshAgent agent;
        
        private TaskStatus taskStatus;

        public override void OnAwake()
        {
            agent = gameObject.GetComponent<NavMeshAgent>();

            _spine = GetComponent<FSpineAnimator>();
            
            supportSphere = neckSupportSphere.Value.transform;
            supportPlatform = bodySupportPlatform.Value.transform;
        }

        public override void OnStart()
        {
            base.OnStart();
            taskStatus = TaskStatus.Running;

            SkillExecute();
        }

        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }

        public override void OnEnd()
        {
            agent.enabled = true;
            _spine.GravityPower = Vector3.zero;
            supportSphere.position = Vector3.down * 10000;
            supportPlatform.position = Vector3.down * 10000;
            
            supportSphere.parent = gameObject.transform;
            supportPlatform.parent = gameObject.transform;
            supportSphere.gameObject.SetActive(false);
            supportPlatform.gameObject.SetActive(false);
        }
        
        private float _t;
        private float _headY;
        private async UniTask SkillExecute()
        {
            // _t += Time.deltaTime/liftTime.Value * 0.5f;
            var position = transform.position;
            // var headPos = new Vector3(position.x, _headY, position.z);
            var originPos = new Vector3(position.x, _originPos.Value.y, position.z);
            // position = Vector3.Lerp(headPos,  originPos, _t);
            // transform.position = position;

            await transform.DOMove(originPos, liftTime.Value)
                .WithCancellation(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            await UniTask.WaitForSeconds(1f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());

            taskStatus = TaskStatus.Success;
        }
    }
}
