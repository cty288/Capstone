using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Enemies;
using Runtime.DataFramework.Properties;
using UnityEngine;

namespace Runtime.DataFramework.ViewControllers.Entities.Enemies {
	public abstract class AbstractEnemyViewController<T> : AbstractCreatureViewController<T, IEnemyEntityModel>, IEnemyViewController 
		where T : class, IEnemyEntity, new() {
		IEnemyEntity IEnemyViewController.EnemyEntity => BindedEntity;
	
		public int Danger {  get; }
	
		public int MaxHealth { get; }
	
		//[Bind(PropertyName.health, nameof(GetCurrentHealth), nameof(OnCurrentHealthChanged))]
		public int CurrentHealth { get; }
		

		protected IEnemyEntityModel enemyEntityModel;

		protected override void Awake() {
			base.Awake();
			
			enemyEntityModel = this.GetModel<IEnemyEntityModel>();
		}

		protected override void OnBindEntityProperty() {
		
			Bind("Danger", BindedEntity.GetDanger());
			Bind<HealthInfo, int>("MaxHealth", BindedEntity.GetHealth(), info => info.MaxHealth);
			Bind<HealthInfo, int>("CurrentHealth", BindedEntity.GetHealth(), info => info.CurrentHealth);
		}

		protected override IEntity OnInitEntity() {
			EnemyBuilder<T> builder = enemyEntityModel.GetEnemyBuilder<T>(1);
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
