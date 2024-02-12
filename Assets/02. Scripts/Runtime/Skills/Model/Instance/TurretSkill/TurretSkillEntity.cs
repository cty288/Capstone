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

			int damage = GetCustomPropertyOfCurrentLevel<int>("damage");
			list.Add(() => new ResourcePropertyDescription(null, Localization.Get(
				"TurretSkill_DAMAGE_NAME"), damage.ToString()));
			
			int ammoSize = GetCustomPropertyOfCurrentLevel<int>("ammo_size");
			list.Add(() => new ResourcePropertyDescription(null,
				Localization.GetFormat("TurretSkill_AMMO_SIZE", ammoSize.ToString()),
				ammoSize.ToString()));
			
			int maxCount = GetCustomPropertyOfCurrentLevel<int>("max_count");
			list.Add(() => new ResourcePropertyDescription(null, null,
				Localization.GetFormat("TurretSkill_MAX_COUNT", maxCount, maxCount > 1 ? "s" : "")));

			if (GetLevel() >= 2) {
				int explodeDamage = GetCustomPropertyOfCurrentLevel<int>("explode_damage");
				list.Add(() => new ResourcePropertyDescription(null, null, Localization.GetFormat(
					"TurretSkill_EXPLODE", explodeDamage.ToString())));
			}

		}

		public override void OnRecycle() {
			base.OnRecycle();
			deployedCount = 0;
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