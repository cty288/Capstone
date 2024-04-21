using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using  Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using UnityEngine;
using UnityEngine.AI;

namespace _02._Scripts.Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BerserkerFlyUp: EnemyAction<BerserkerEntity>
    {
        private TaskStatus taskStatus;
        private NavMeshAgent agent;

        public SharedBool isFlying;
        public SharedGameObject finalPosition;
        
        private float speed;

        public override void OnAwake()
        {
            base.OnAwake();
            agent = gameObject.GetComponent<NavMeshAgent>();
        }

        public override void OnStart()
        {
            base.OnStart();
            speed = enemyEntity.GetCustomDataValue<float>("entity", "speed");
            agent.enabled = false;
            
            taskStatus = TaskStatus.Running;
            SkillExecute();
        }
        
        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }
        
        public async UniTask SkillExecute()
        {
            float duration = (enemyEntity.Nodes[0].transform.position.y - transform.position.y) / speed;
                
            await transform.DOMoveY(enemyEntity.Nodes[0].transform.position.y,duration)
                .WithCancellation(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            await UniTask.WaitForSeconds(0.25f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            isFlying.Value = true;
            
            BerserkerNode node = enemyEntity.Nodes[Random.Range(0, enemyEntity.Nodes.Count)];
            finalPosition.Value = node.gameObject;
            duration = Vector3.Distance(node.transform.position, transform.position) / speed;
            
            Quaternion direction = Quaternion.LookRotation(node.transform.position - transform.position);
            
            transform.DORotateQuaternion(direction, 0.2f).WithCancellation(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            await transform.DOMove(node.transform.position, duration).SetEase(Ease.Linear)
                .WithCancellation(cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            await UniTask.WaitForSeconds(2f,
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            taskStatus = TaskStatus.Success;
        }
        
        public override void OnEnd()
        {
            base.OnEnd();
        }
    }
}