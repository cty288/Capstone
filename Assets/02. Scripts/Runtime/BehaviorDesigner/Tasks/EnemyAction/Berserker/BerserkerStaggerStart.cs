using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using  Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.Enemies.ViewControllers.Instances.Berserker;
using UnityEngine;

namespace _02._Scripts.Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class BerserkerStaggerStart : EnemyAction<BerserkerEntity>
    {
        private TaskStatus taskStatus = TaskStatus.Running;

        public SharedBool isFlying;
        
        private Rigidbody rb;
        private bool isFalling;

        public override void OnStart()
        {
            base.OnStart();
            rb = GetComponent<Rigidbody>();
            Debug.Log($"BERSERKER: stagger start");
            enemyEntity.SetStaggerStatus(true);

            SkillExecute();
        }

        private async UniTask SkillExecute()
        {
            
            //play stagger start animation
            // wait
            await UniTask.WaitForSeconds(0.5f, 
                cancellationToken: gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
            
            rb.useGravity = true;
            rb.isKinematic = false;
            isFalling = true;
        }
        
        public override TaskStatus OnUpdate()
        {
            if(rb.useGravity && !isFalling)
            {
                Debug.Log($"BERSERKER: stagger hit ground");

                rb.useGravity = false;
                rb.isKinematic = true;

                // change to stagger animation
                isFlying.Value = false;
                taskStatus = TaskStatus.Success;
            }

            return taskStatus;
        }

        public override void OnFixedUpdate()
        {
            if (isFalling)
            {
                // RaycastHit hit;
                int layerMask = 1 << 8; // anything
                if (Physics.Raycast(transform.position, Vector3.down, out _, 0.1f,
                        layerMask))
                {
                    isFalling = false;
                }
            }
        }
    }
}