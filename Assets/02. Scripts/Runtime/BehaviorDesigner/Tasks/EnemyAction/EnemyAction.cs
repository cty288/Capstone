using BehaviorDesigner.Runtime.Tasks;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using UnityEngine;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public abstract class EnemyAction : Action
    {
        protected Rigidbody body;
        protected Animator anim;
        protected IEnemyViewController enemyViewController;

        public override void OnAwake()
        {
            body = GetComponent<Rigidbody>();
            anim = gameObject.GetComponent<Animator>();
            enemyViewController = this.gameObject.GetComponent<IEnemyViewController>();
        }
        
       // protected abstract IEnemyViewController GetEnemyViewController();
    }


    public abstract class EnemyAction<T> : EnemyAction where T : class, IEnemyEntity {
        protected T enemyEntity;

        public override void OnAwake() {
            base.OnAwake();
            enemyEntity = enemyViewController.EnemyEntity as T;
        }
    }
    
    
}
