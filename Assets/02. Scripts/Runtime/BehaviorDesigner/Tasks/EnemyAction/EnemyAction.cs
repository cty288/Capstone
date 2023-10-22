using BehaviorDesigner.Runtime.Tasks;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Temporary;
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
            if (anim == null)
            {
                anim = gameObject.GetComponentInChildren<Animator>();
            }
            enemyViewController = this.gameObject.GetComponent<IEnemyViewController>();
        }

        public GameObject GetPlayer() {
            return PlayerController.GetClosestPlayer(transform.position)?.gameObject;
        }
        
       // protected abstract IEnemyViewController GetEnemyViewController();
    }


    public abstract class EnemyAction<T> : EnemyAction where T : class, IEnemyEntity {
        protected T enemyEntity;

        public override void OnAwake() {
            base.OnAwake();
            
        }

        public override void OnStart() {
            base.OnStart();
            enemyEntity = enemyViewController.EnemyEntity as T;
        }
    }
    
    
}
