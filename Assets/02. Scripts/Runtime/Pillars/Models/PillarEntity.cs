using System;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.Properties;
using AYellowpaper.SerializedCollections;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Spawning {
	[Serializable]
	public class RewardCostInfo {
		//public CurrencyType CurrencyType;
		
		[SerializedDictionary("Level", "Cost")]
		public SerializedDictionary<int, int> Costs;
		
		public int GetCostOfLevel(int level) {
			if (Costs.ContainsKey(level)) {
				return Costs[level];
			}
			//find the closest level
			int closestLevel = 0;
			int closestLevelDistance = int.MaxValue;
			foreach (var cost in Costs) {
				if (Math.Abs(cost.Key - level) < closestLevelDistance) {
					closestLevel = cost.Key;
					closestLevelDistance = Math.Abs(cost.Key - level);
				}
			}

			return Costs[closestLevel];
		}
	}

	public interface IPillarEntity : IEntity, IHaveCustomProperties, IHaveTags {
		CurrencyType PillarCurrencyType { get; set; }
		
		RewardCostInfo RewardCost { get; set; }
	}
	public class PillarEntity : AbstractBasicEntity, IPillarEntity{
		
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Pillar";
		
		public CurrencyType PillarCurrencyType { get; set; }
		
		public RewardCostInfo RewardCost { get; set; }

		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnDoRecycle() {
			SafeObjectPool<PillarEntity>.Singleton.Recycle(this);
		}

		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			this.RegisterInitialProperty<ILevelNumberProperty>(new LevelNumber());
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}

		

		
	}
}