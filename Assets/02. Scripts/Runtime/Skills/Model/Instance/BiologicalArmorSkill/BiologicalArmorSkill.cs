using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Inventory.Model;
using Runtime.Player;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.Model.Instance.BiologicalArmorSkill {
	public class BiologicalArmorSkill : PassiveSkillEntity<BiologicalArmorSkill>, ICanGetModel  {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "BiologicalArmorSkill";
		
		[field: ES3Serializable]
		private int alreadyAddedArmor = 0;

		[field: ES3Serializable] 
		private float alreadyAddedArmorRecoverSpeed = 0;
		
		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable { get; } = true;
		protected override string GetDescription(string defaultLocalizationKey) {
			string additionalDesc = "";
			if (GetLevel() >= 3) {
				float addedArmor = GetCustomPropertyOfCurrentLevel<float>("added_armor_recover_speed");
				additionalDesc = "\n\n" + Localization.GetFormat("BiologicalArmorSkill_desc2", addedArmor);
			}

			int addAmount = GetCustomPropertyOfCurrentLevel<int>("add_amount");
			int healthLimit = GetCustomPropertyOfCurrentLevel<int>("health_limit");
			return Localization.GetFormat(defaultLocalizationKey, addAmount, healthLimit, additionalDesc);
		}

		protected override void OnUpgrade(int previousLevel, int level) {
			AddArmorOfLevel(level);
		}

		public override void OnRemovedFromInventory() {
			base.OnRemovedFromInventory();
			IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
			playerEntity.ChangeMaxArmor(-alreadyAddedArmor);
			playerEntity.GetArmorRecoverSpeed().RealValue.Value -= alreadyAddedArmorRecoverSpeed;
			playerEntity.UnRegisterOnModifyReceivedHealAmount(OnModifyHealAmount);
		}

		private int OnModifyHealAmount(int amount, IBelongToFaction arg2, IDamageable target) {
			int healthLimit = GetCustomPropertyOfCurrentLevel<int>("health_limit");
			int targetCurrentHealth = target.GetCurrentHealth();
			if (targetCurrentHealth + amount > healthLimit) {
				return Mathf.Max(0, healthLimit - targetCurrentHealth);
			}
			return amount;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			base.OnEntityStart(isLoadedFromSave);
			if (isInInventory) {
				IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
				playerEntity.RegisterOnModifyReceivedHealAmount(OnModifyHealAmount);
			}
		}

		public override void OnAddedToInventory(string playerUUID) {
			base.OnAddedToInventory(playerUUID);
			AddArmorOfLevel(GetLevel());
			IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
			playerEntity.RegisterOnModifyReceivedHealAmount(OnModifyHealAmount); 
		}

		private void AddArmorOfLevel(int level) {
			int addAmount = GetCustomPropertyWithLevel<int>("add_amount", level);
			int realAddAmount = addAmount - alreadyAddedArmor;
			if (realAddAmount < 0) return;
			
			alreadyAddedArmor = addAmount;
			IPlayerEntity playerEntity = this.GetModel<IGamePlayerModel>().GetPlayer();
			playerEntity.ChangeMaxArmor(realAddAmount);
			
			if(level >= 3) {
				float addedArmorRecoverSpeed = GetCustomPropertyWithLevel<float>("added_armor_recover_speed", level);
				float realAddedArmorRecoverSpeed = addedArmorRecoverSpeed - alreadyAddedArmorRecoverSpeed;
				alreadyAddedArmorRecoverSpeed = addedArmorRecoverSpeed;

				playerEntity.GetArmorRecoverSpeed().RealValue.Value += realAddedArmorRecoverSpeed;
			}
		}

		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			alreadyAddedArmor = 0;
			alreadyAddedArmorRecoverSpeed = 0;
		}
	}
}