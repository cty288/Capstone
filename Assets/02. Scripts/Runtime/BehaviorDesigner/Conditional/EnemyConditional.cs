using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Temporary;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Conditional
{
    public class EnemyConditional : global::BehaviorDesigner.Runtime.Tasks.Conditional
    {
        protected Rigidbody2D body;
        protected Animator animator;
        public SharedBool condition;
        protected IEnemyViewController enemyViewController;
        //further implementation
        //protected Destructable destructable;
        //protected PlayerController player;

        public override void OnAwake()
        {

            body = GetComponent<Rigidbody2D>();
            //player = PlayerController.Instance;
            //destructable = GetComponent<Destructable>();
            animator = gameObject.GetComponentInChildren<Animator>();
            enemyViewController = this.gameObject.GetComponent<IEnemyViewController>();
        }
        
        public GameObject GetPlayer() {
            return PlayerController.GetClosestPlayer(transform.position)?.gameObject;
        }

        public override TaskStatus OnUpdate()
        {
            if (condition.Value) return TaskStatus.Success;
            else return TaskStatus.Failure;
        }
    }
    
    public abstract class EnemyConditional<T> : EnemyConditional where T : class, IEnemyEntity {
        protected T enemyEntity;
        
        public override void OnStart() {
            base.OnStart();
            enemyEntity = enemyViewController.EnemyEntity as T;
        }
    }
}