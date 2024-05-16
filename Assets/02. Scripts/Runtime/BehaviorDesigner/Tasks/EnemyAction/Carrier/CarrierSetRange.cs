using System.Collections;
using a;
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
	public class CarrierSetRange : EnemyAction<QuadrupedCarrierEntity>
	{
		public SharedFloat range;


		public override void OnStart()
		{
			base.OnStart();
			range.Value = enemyEntity.GetCustomDataValue<float>("attack", "detectionRange");
		}

		public override TaskStatus OnUpdate()
		{
			return TaskStatus.Success;
		}
	}
}