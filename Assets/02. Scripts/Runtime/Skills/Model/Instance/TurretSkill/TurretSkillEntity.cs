using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill {
	public class TurretSkillEntity : SkillEntity<TurretSkillEntity> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TurretSkill";
		public override string DeployedVCPrefabName { get; } = "Turret_Deployed";
		
		private int deployedCount = 0;
		
		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable { get; } = true;
		protected override string GetDescription(string defaultLocalizationKey) {
			float time = GetCustomPropertyOfCurrentLevel<float>("last_time");
			int displayTime = Mathf.RoundToInt(time);
			return Localization.GetFormat(defaultLocalizationKey, displayTime);
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
		
		public override void OnRegisterResourcePropertyDescriptionGetters(ref List<GetResourcePropertyDescriptionGetter> list) {
			base.OnRegisterResourcePropertyDescriptionGetters(ref list);

			
			list.Add(() => {
				int damage = GetCustomPropertyOfCurrentLevel<int>("damage");
				return new ResourcePropertyDescription(null, Localization.Get(
					"TurretSkill_DAMAGE_NAME"), damage.ToString());
			});
			
			
			list.Add(() => {
				int ammoSize = GetCustomPropertyOfCurrentLevel<int>("ammo_size");
				return new ResourcePropertyDescription(null,
					Localization.GetFormat("TurretSkill_AMMO_SIZE", ammoSize.ToString()),
					ammoSize.ToString());
			});
			
			
			list.Add(() => {
				int maxCount = GetCustomPropertyOfCurrentLevel<int>("max_count");
				return new ResourcePropertyDescription(null, null,
					Localization.GetFormat("TurretSkill_MAX_COUNT", maxCount, maxCount > 1 ? "s" : ""));
			});

			
			list.Add(() => {
				if (GetLevel() >= 2) {
					int explodeDamage = GetCustomPropertyOfCurrentLevel<int>("explode_damage");
					return new ResourcePropertyDescription(null, null, Localization.GetFormat(
						"TurretSkill_EXPLODE", explodeDamage.ToString()));
				}
				else {
					return new ResourcePropertyDescription(null, null, null, false);
				}
			});
			

		}

		public override void OnRecycle() {
			base.OnRecycle();
			deployedCount = 0;
		}


		protected override void OnUpgrade(int previousLevel, int level) {
			
		}

		protected override bool UseCurrencySatisfiedCondition(Dictionary<CurrencyType, int> currency) {
			int maxCount = GetCustomPropertyOfCurrentLevel<int>("max_count");
			return base.UseCurrencySatisfiedCondition(currency) && deployedCount < maxCount;
		}

		public void OnSpawnTurret(TurretEntity entity) {
			if(entity == null) return;
			
			deployedCount++;
			entity.RegisterOnEntityRecycled(OnTurretRecycled);
		}

		private void OnTurretRecycled(IEntity e) {
			e.UnRegisterOnEntityRecycled(OnTurretRecycled);
			deployedCount--;
		}
	}
}