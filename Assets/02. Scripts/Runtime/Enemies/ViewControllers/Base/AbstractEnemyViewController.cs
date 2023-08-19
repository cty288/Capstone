using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.Model.Properties;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Base {
	public abstract class AbstractEnemyViewController<T> : AbstractCreatureViewController<T, IEnemyEntityModel>, IEnemyViewController 
		where T : class, IEnemyEntity, new() {
		IEnemyEntity IEnemyViewController.EnemyEntity => BoundEntity;
	
		public int Danger {  get; }
	
		public int MaxHealth { get; }
	
		//[Bind(PropertyName.health, nameof(GetCurrentHealth), nameof(OnCurrentHealthChanged))]
		public int CurrentHealth { get; }
		

		
		protected override void OnBindEntityProperty() {
		
			Bind("Danger", BoundEntity.GetDanger());
			Bind<HealthInfo, int>("MaxHealth", BoundEntity.GetHealth(), info => info.MaxHealth);
			Bind<HealthInfo, int>("CurrentHealth", BoundEntity.GetHealth(), info => info.CurrentHealth);
		}

		protected override IEntity OnInitEntity() {
			EnemyBuilder<T> builder = entityModel.GetEnemyBuilder<T>(1);
			return OnInitEnemyEntity(builder);
		}

		protected abstract IEnemyEntity OnInitEnemyEntity(EnemyBuilder<T> builder);

		protected dynamic GetMaxHealth(dynamic info) {
			return info.MaxHealth;
		}
	
		protected dynamic GetCurrentHealth(dynamic info) {
			return info.CurrentHealth;
		}
	
		protected void OnCurrentHealthChanged(int oldValue, int newValue) {
			Debug.Log("CurrentHealth changed from " + oldValue + " to " + newValue);
		}
	}
}
