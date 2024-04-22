using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using  Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using UnityEngine;

namespace _02._Scripts.Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BerserkerStaggerEnd : EnemyAction<BerserkerEntity>
    {
        private TaskStatus taskStatus = TaskStatus.Running;
        
        public override void OnStart()
        {
            base.OnStart();
            Debug.Log($"BERSERKER: start end stagger");
            SkillExecute();
        }

        private async UniTask SkillExecute()
        {
            //play get up animation
            // wait
            await UniTask.WaitForSeconds(0.5f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            Debug.Log($"BERSERKER: end stagger");

            enemyEntity.SetStaggerStatus(false);
            taskStatus = TaskStatus.Success;
        }

        public override TaskStatus OnUpdate()
        {
            return taskStatus;
        }
    }
}