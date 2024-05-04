using _02._Scripts.Runtime.CollectableResources.Model;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.CollectableResources.ViewControllers {
	public class PlantEntity : CollectableResourceEntity<PlantEntity> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Plant";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnRecycle() {
			
		}

		protected override void OnInitModifiers(int level, int rarity = 1) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return new ICustomProperty[] {
				new AutoConfigCustomProperty("explosion")
			};
		}
	}
	public class PlantResourceViewController : CollectableResourceViewController<PlantEntity> {
		protected SafeGameObjectPool explosionPool;
		[SerializeField] private GameObject explosionPrefab;
		protected override void Awake() {
			base.Awake();
			explosionPool = GameObjectPoolManager.Singleton.CreatePool(explosionPrefab, 30, 60);
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override void GenerateResources() {
			base.GenerateResources();
			GameObject exp = explosionPool.Allocate();
			
			exp.transform.position = transform.position;
			exp.transform.rotation = Quaternion.identity;

			int damage = BoundEntity.GetCustomDataValue<int>("explosion", "damage");
			float distance = BoundEntity.GetCustomDataValue<float>("explosion", "distance");
			exp.GetComponent<IExplosionViewController>().Init
				(Faction.Explosion, damage, distance, null, null);
		}
	}
}