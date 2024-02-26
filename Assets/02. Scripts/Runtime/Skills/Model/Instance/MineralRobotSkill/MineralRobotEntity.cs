using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Skills.Model.Instance.MineralRobotSkill {

	public enum MineralBotCollectResource {
		None,
		Combat,
		Mineral,
		Plant,
		Time
	}
	
	public static class MineralBotCollectResourceExtension {
		public static CurrencyType ConvertToCurrencyType(this MineralBotCollectResource resource) {
			switch (resource) {
				case MineralBotCollectResource.Combat:
					return CurrencyType.Combat;
				case MineralBotCollectResource.Mineral:
					return CurrencyType.Mineral;
				case MineralBotCollectResource.Plant:
					return CurrencyType.Plant;
				case MineralBotCollectResource.Time:
					return CurrencyType.Time;
				default:
					return CurrencyType.Combat;
			}
		}
		
		public static MineralBotCollectResource ConvertToMineralBotCollectResource(this CurrencyType currencyType) {
			switch (currencyType) {
				case CurrencyType.Combat:
					return MineralBotCollectResource.Combat;
				case CurrencyType.Mineral:
					return MineralBotCollectResource.Mineral;
				case CurrencyType.Plant:
					return MineralBotCollectResource.Plant;
				case CurrencyType.Time:
					return MineralBotCollectResource.Time;
				default:
					return MineralBotCollectResource.None;
			}
		}
	}
	
	public class MineralRobotEntity  : AbstractBasicEntity{
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "MineralRobotEntity";

		[field: ES3Serializable]
		public BindableProperty<MineralBotCollectResource> CollectResourceType { get; private set; } =
			new BindableProperty<MineralBotCollectResource>(MineralBotCollectResource.None);

		[field: ES3Serializable]
		public BindableProperty<int> CollectedCount { get; private set; } = new BindableProperty<int>(0);
		
		
		
		private BindableProperty<int> limit; 
		public BindableProperty<int> Limit => limit;
		
		
		private BindableProperty<float> interval; 
		public BindableProperty<float> Interval => interval;
		
		
		
		private BindableProperty<int> count;
		public BindableProperty<int> ResourceCollectCountPerInterval => count;

		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			limit = GetCustomDataValue<int>("data", "limit");
			interval = GetCustomDataValue<float>("data", "interval");
			count = GetCustomDataValue<int>("data", "count");
		}

		public override void OnDoRecycle() {
			SafeObjectPool<MineralRobotEntity>.Singleton.Recycle(this);
		}

		public override void OnRecycle() {
			CollectResourceType.Value = MineralBotCollectResource.None;
			CollectedCount.Value = 0;
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
					new CustomDataProperty<int>("limit"),
					new CustomDataProperty<float>("interval"),
					new CustomDataProperty<int>("count"))
			};
		}
	}
}