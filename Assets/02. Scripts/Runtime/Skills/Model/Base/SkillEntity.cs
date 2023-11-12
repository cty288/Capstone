using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Properties;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Properties;
using UnityEngine;
using UnityEngine.PlayerLoop;
using AutoConfigCustomProperty = Runtime.DataFramework.Properties.CustomProperties.AutoConfigCustomProperty;

namespace _02._Scripts.Runtime.Skills.Model.Base {
	public interface ISkillEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
		public float GetRemainingCooldown();
		
		public float GetMaxCooldown();

		public bool HasCooldown();
		
		public void SetRemainingCooldown(float remainingCooldown);
		
		public void StartSwapInventoryCooldown(float cooldown);
		public BindableProperty<T> GetCustomPropertyWithLevel<T>(string propertyName, int level);
		
		Func<Dictionary<CurrencyType, int>, bool> GetUseCurrencySatisfiedCondition { get; }

		void UseSkill();
	}

	public struct OnSkillUsed {
		public ISkillEntity skillEntity;
	}
	public abstract class SkillEntity<T>:  ResourceEntity<T>, ISkillEntity  where T : SkillEntity<T>, new() {
		protected ISkillCoolDown skillCooldownProperty;
		protected ISkillUseCost skillUseCostProperty;
		protected ISkillUpgradeCost skillUpgradeCostProperty;
		protected ISkillPurchaseCost skillPurchaseCostProperty;


		public override string InHandVCPrefabName => EntityName;

		[ES3Serializable] 
		private float remainingCooldown = 0;
		private float maxCooldown = 0;
		protected bool isWaitingForSwapInventoryCooldown = false;
		protected virtual int levelRange { get; } = 4;
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.SkillEntityConfigTable;
		}
		
		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.Skill;
		}

		protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
			return originalDisplayName;
		}

		protected override void OnRegisterProperties() {
			base.OnRegisterProperties();
			RegisterInitialProperty<ISkillCoolDown>(new SkillCooldown());
			RegisterInitialProperty<ISkillPurchaseCost>(new SkillPurchaseCost());
			RegisterInitialProperty<ISkillUpgradeCost>(new SkillUpgradeCost());
			RegisterInitialProperty<ISkillUseCost>(new SkillUseCost());
		}

		public override void OnAwake() {
			base.OnAwake();
			skillCooldownProperty = GetProperty<ISkillCoolDown>();
			skillUseCostProperty = GetProperty<ISkillUseCost>();
			skillUpgradeCostProperty = GetProperty<ISkillUpgradeCost>();
			skillPurchaseCostProperty = GetProperty<ISkillPurchaseCost>();
			
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			maxCooldown = skillCooldownProperty.GetByLevel(GetRarity());
			if (!isLoadedFromSave) {
				remainingCooldown = maxCooldown;
			}

			if (maxCooldown > 0 && HasCooldown()) {
				CoroutineRunner.Singleton.RegisterUpdate(OnUpdate);
			}
		}

		private void OnUpdate() {
			if (remainingCooldown > 0) {
				remainingCooldown -= Time.deltaTime;
				if (remainingCooldown < 0) {
					remainingCooldown = 0;
					if (isWaitingForSwapInventoryCooldown) {
						isWaitingForSwapInventoryCooldown = false;
						maxCooldown = skillCooldownProperty.GetByLevel(GetRarity());
					}
				}
			}
		}

		public override void OnRecycle() {
			CoroutineRunner.Singleton.UnregisterUpdate(OnUpdate);
		}

		public override string OnGroundVCPrefabName { get; } = null;

		[field: ES3Serializable]
		public string InventoryVCPrefabName { get; } = "SkillInventoryVC";


		

		public float GetRemainingCooldown() {
			return remainingCooldown;
		}

		public float GetMaxCooldown() {
			return maxCooldown;
		}

		public virtual bool HasCooldown() {
			return true;
		}

		public void SetRemainingCooldown(float remainingCooldown) {
			this.remainingCooldown = remainingCooldown;
		}

		
		public void StartSwapInventoryCooldown(float cooldown) {
			if (!HasCooldown()) {
				return;
			}
			isWaitingForSwapInventoryCooldown = true;
			remainingCooldown = Mathf.Max(remainingCooldown, cooldown);
			maxCooldown = remainingCooldown;
		}

		public BindableProperty<T1> GetCustomPropertyWithLevel<T1>(string propertyName, int level) {
			int targetLevel = level + 1;
			while (targetLevel > 0) {
				targetLevel--;
				string name = $"level{targetLevel}";
				if (!HasCustomProperty(name)) {
					continue;
				}

				if (GetCustomProperties()[name].TryGetCustomDataValue<T1>(propertyName, out BindableProperty<T1> val)) {
					return val;
				}
			}
			return null;
		}

		public Func<Dictionary<CurrencyType, int>, bool> GetUseCurrencySatisfiedCondition =>
			UseCurrencySatisfiedCondition;

		public void UseSkill() {
			remainingCooldown = maxCooldown;
			this.SendEvent<OnSkillUsed>(new OnSkillUsed() {
				skillEntity = this
			});
		}

		public Func<Dictionary<CurrencyType, int>, bool> CanInventorySwitchToCondition => GetInventorySwitchCondition;

		protected virtual bool GetInventorySwitchCondition(Dictionary<CurrencyType, int> currency) {
			if(remainingCooldown > 0) {
				return false;
			}

			return UseCurrencySatisfiedCondition(currency);
		}
		
		protected virtual bool UseCurrencySatisfiedCondition(Dictionary<CurrencyType, int> currency) {
			int rarity = GetRarity();
			foreach (CurrencyType currencyType in currency.Keys) {
				if(skillUseCostProperty.GetByLevel(rarity, currencyType) > currency[currencyType]) {
					return false;
				}
			}

			return true;
		}

		
		
		protected override ICustomProperty[] OnRegisterCustomProperties() {
			AutoConfigCustomProperty[] properties = new AutoConfigCustomProperty[levelRange];
			for (int i = 1; i <= levelRange; i++) {
				properties[i - 1] = new AutoConfigCustomProperty($"level{i}");
			}
			
			ICustomProperty[] additionalProperties = OnRegisterAdditionalCustomProperties();
			if (additionalProperties != null) {
				ICustomProperty[] result = new ICustomProperty[properties.Length + additionalProperties.Length];
				properties.CopyTo(result, 0);
				additionalProperties.CopyTo(result, properties.Length);
				return result;
			}

			ICustomProperty[] res= new ICustomProperty[properties.Length];
			properties.CopyTo(res, 0);
			return res;
		}
		
		protected abstract ICustomProperty[] OnRegisterAdditionalCustomProperties();
	}
}