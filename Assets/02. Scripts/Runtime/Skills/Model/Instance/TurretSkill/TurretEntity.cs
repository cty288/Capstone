using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill {
	public class TurretEntity : AbstractBasicEntity {
		
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TurretEntity";
		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnDoRecycle() {
			SafeObjectPool<TurretEntity>.Singleton.Recycle(this);
		}

		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return new[] {
				new CustomProperty("data",
					new CustomDataProperty<float>("install_time"),
					new CustomDataProperty<float>("vision"),
					new CustomDataProperty<float>("last_time"),
					new CustomDataProperty<int>("ammo_size"),
					new CustomDataProperty<float>("time_per_shot"),
					new CustomDataProperty<int>("damage"))
			};

		}
	}
}