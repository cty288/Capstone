using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using UnityEngine;
using Runtime.Temporary.Weapon;
using Runtime.Weapons.ViewControllers.Base;
using Runtime.Enemies.SmallEnemies;



namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public class DroneExplode : EnemyAction<BeeEntity>
{
        // Start is called before the first frame update
        public SharedGameObject explosion;
        private SafeGameObjectPool pool;

        public override void OnAwake()
        {
            base.OnAwake();
            pool = GameObjectPoolManager.Singleton.CreatePool(explosion.Value, 20, 50);
        }

        public override TaskStatus OnUpdate()
        {
            Debug.Log("exploding");
            GameObject explosion = pool.Allocate();
            explosion.transform.position = this.gameObject.transform.position;
            explosion.GetComponent<AbstractExplosionViewController>().Init(enemyEntity.CurrentFaction.Value, enemyEntity.GetCustomDataValue<int>("attack", "explosionDamage"), gameObject,
                gameObject.GetComponent<ICanDealDamage>());
            return TaskStatus.Success;
           
        }
    }
}
