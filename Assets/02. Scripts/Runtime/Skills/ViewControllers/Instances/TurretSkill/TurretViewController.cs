using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.TurretSkill {
	public class TurretViewController : AbstractBasicEntityViewController<TurretEntity> {
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = true;
		
		protected ICommonEntityModel commonEntityModel;
		
		protected override void Awake() {
			base.Awake();
			commonEntityModel = this.GetModel<ICommonEntityModel>();
		}

		public TurretEntity OnBuildTurretEntity(float installTime, float vision, float lastTime, int ammoSize,
			float timePerShot, int damage) {
			if (commonEntityModel == null) {
				commonEntityModel = this.GetModel<ICommonEntityModel>();
			}
			var builder = commonEntityModel.GetBuilder<TurretEntity>(1);
			builder.SetProperty<float>(new PropertyNameInfo("data", "install_time"), installTime)
				.SetProperty<float>(new PropertyNameInfo("data", "vision"), vision)
				.SetProperty<float>(new PropertyNameInfo("data", "last_time"), lastTime)
				.SetProperty<int>(new PropertyNameInfo("data", "ammo_size"), ammoSize)
				.SetProperty<float>(new PropertyNameInfo("data", "time_per_shot"), timePerShot)
				.SetProperty<int>(new PropertyNameInfo("data", "damage"), damage);
			
			return builder.Build();
		}
		
		
		protected override IEntity OnBuildNewEntity() {
			return OnBuildTurretEntity(5, 30, 60, 50, 0.2f, 2);
		}

		protected override void OnEntityStart() {
			Debug.Log(
				$"Turret Deployed! Install time: {BoundEntity.GetCustomDataValue<float>("data", "install_time")}\n" +
				$"Vision: {BoundEntity.GetCustomDataValue<float>("data", "vision")}\n" +
				$"Last time: {BoundEntity.GetCustomDataValue<float>("data", "last_time")}\n" +
				$"Ammo size: {BoundEntity.GetCustomDataValue<int>("data", "ammo_size")}\n" +
				$"Time per shot: {BoundEntity.GetCustomDataValue<float>("data", "time_per_shot")}\n" +
				$"Damage: {BoundEntity.GetCustomDataValue<int>("data", "damage")}");
		}

		protected override void OnBindEntityProperty() {
			
		}
	}
}