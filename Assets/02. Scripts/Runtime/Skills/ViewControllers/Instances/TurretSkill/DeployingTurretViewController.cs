using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.TurretSkill {
	public class DeployingTurretViewController : AbstractDeployableResourceViewController<TurretSkillEntity> {
		[SerializeField] private GameObject turretPrefab;
		protected override void OnEntityStart() {
			
		}

		protected override void OnBindEntityProperty() {
		
		}

		public override void OnDeployed() {
			Debug.Log("Turret Deployed!!");
			//TODO: destroy this vc, then spawn a turret vc
			SpawnTurret();
			RecycleToCache();
		}

		private void SpawnTurret() {
			var turret = Instantiate(turretPrefab, transform.position, transform.rotation);
			TurretEntity entity = turret.GetComponent<TurretViewController>().OnBuildTurretEntity(
				BoundEntity.GetCustomPropertyOfCurrentLevel<float>("install_time"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<float>("vision"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<float>("last_time"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<int>("ammo_size"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<float>("time_per_shot"),
				BoundEntity.GetCustomPropertyOfCurrentLevel<int>("damage"));

			turret.GetComponent<TurretViewController>().InitWithID(entity.UUID);
		}

		protected override IResourceEntity OnBuildNewEntity(bool isPreview) {
			return null;
		}
	}
}