using System;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.Properties;
using AYellowpaper.SerializedCollections;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Spawning {
	public enum PillarStatus {
		Idle,
		Activated,
		Spawning,
		Deactivated
	}
	
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

		public int GetLevel(int cost) { //the key are ordered by ascending order, find the largest key that is smaller than cost
			int closestLevel = -1;
			int closestLevelDistance = int.MaxValue;
			foreach (var costPair in Costs) {
				if (costPair.Value <= cost && Math.Abs(costPair.Value - cost) < closestLevelDistance) {
					closestLevel = costPair.Key;
					closestLevelDistance = Math.Abs(costPair.Value - cost);
				}
			}

			return closestLevel;

		}
		public int GetHighestCost() {
			int highestCost = 0;
			foreach (var cost in Costs) {
				if (cost.Value > highestCost) {
					highestCost = cost.Value;
				}
			}

			return highestCost;
		}
	}

	public interface IPillarEntity : IEntity, IHaveCustomProperties, IHaveTags {
		CurrencyType PillarCurrencyType { get; set; }
		
		RewardCostInfo RewardCost { get; set; }
		
		BindableProperty<PillarStatus> Status { get; }
	}
	public class PillarEntity : AbstractBasicEntity, IPillarEntity{
		
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Pillar";
		
		public CurrencyType PillarCurrencyType { get; set; }
		
		public RewardCostInfo RewardCost { get; set; }
		
		public BindableProperty<PillarStatus> Status { get; } = new BindableProperty<PillarStatus>(PillarStatus.Idle);

		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		public override void OnDoRecycle() {
			SafeObjectPool<PillarEntity>.Singleton.Recycle(this);
		}

		public override void OnRecycle() {
			Status.Value = PillarStatus.Idle;
			RewardCost = null;
			PillarCurrencyType = CurrencyType.Combat;
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