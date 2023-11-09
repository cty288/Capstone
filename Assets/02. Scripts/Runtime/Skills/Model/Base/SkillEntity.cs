using _02._Scripts.Runtime.Skills.Model.Properties;
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

namespace _02._Scripts.Runtime.Skills.Model.Base {
	public interface ISkillEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
		public float GetRemainingCooldown();
		
		public float GetMaxCooldown();
		
		public void SetRemainingCooldown(float remainingCooldown);
	}
	public abstract class SkillEntity<T>:  ResourceEntity<T>, ISkillEntity  where T : SkillEntity<T>, new() {
		protected ISkillCoolDown skillCooldownProperty;

		[ES3Serializable] 
		private float remainingCooldown = 0;
		private float maxCooldown = 0;
		
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
		}

		public override void OnAwake() {
			base.OnAwake();
			skillCooldownProperty = GetProperty<ISkillCoolDown>();
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			maxCooldown = skillCooldownProperty.GetMaxCooldownByLevel(GetRarity());
			if (!isLoadedFromSave) {
				remainingCooldown = maxCooldown;
			}
			CoroutineRunner.Singleton.RegisterUpdate(OnUpdate);
		}

		private void OnUpdate() {
			if (remainingCooldown > 0) {
				remainingCooldown -= Time.deltaTime;
				if (remainingCooldown < 0) {
					remainingCooldown = 0;
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

		public void SetRemainingCooldown(float remainingCooldown) {
			this.remainingCooldown = remainingCooldown;
		}
	}
}