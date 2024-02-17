using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Player;
using Runtime.Player.Properties;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.Model.Instances.AdrenalineSkill {
	public class StimulatedBuff : ConfigurableBuff<StimulatedBuff> {
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = 10;
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = -1;
		public override int Priority { get; } = 1;
		
		private IPlayerEntity playerEntity;
		
		[field: ES3Serializable]
		private float addedWalkSpeed;
		[field: ES3Serializable]
		private float addedSprintSpeed;
		[field: ES3Serializable]
		private float addedSlideSpeed;
		
		private int addedTempHealth;
		public override bool IsDisplayed() {
			return true;
		}
		
		public override string GetLevelDescription(int level) {
			int displayedDamage = (int) (GetBuffPropertyAtLevel<float>("damage", level) * 100);
			int displayedMovementSpeed = (int) (GetBuffPropertyAtLevel<float>("move_speed", level) * 100);
			int tempHealth = GetBuffPropertyAtLevel<int>("temp_health", level);
			int displayedDuration = (int) GetBuffPropertyAtLevel<float>("time", level);

			return Localization.GetFormat("StimulatedBuff_Desc", level, displayedDamage,
				displayedMovementSpeed, tempHealth, displayedDuration);
		}

		public override void OnInitialize() {
			playerEntity = buffOwner as IPlayerEntity;
			if (playerEntity != null) {
				playerEntity.RegisterOnModifyDamageCount(OnPlayerModifyDamageCount);
				playerEntity.HealthProperty.RealValue.RegisterOnValueChanged(OnPlayerHealthChanged);
			}
			
		}

		private void OnPlayerHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
			int change = newHealth.CurrentHealth - oldHealth.CurrentHealth;
			if (change < 0) {
				addedTempHealth -= Mathf.Abs(change);
			}
		}
		

		private int OnPlayerModifyDamageCount(int count) {
			float damageMultiplier = GetBuffPropertyAtCurrentLevel<float>("damage");
			return Mathf.CeilToInt(count * (1 + damageMultiplier));
		}

		public override void OnStart() {
			AddSpeed();
			int tempHealth = GetBuffPropertyAtCurrentLevel<int>("temp_health");
			int currentHealth = playerEntity.GetCurrentHealth();
			int maxHealth = playerEntity.GetMaxHealth();

			addedTempHealth = Mathf.Min(maxHealth - currentHealth, tempHealth);
			playerEntity.Heal(addedTempHealth, playerEntity);
		}

		

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override bool IsGoodBuff => true;
		public override void OnBuffEnd() {
			RecoverSpeed();
			if (addedTempHealth > 0) {
				playerEntity.ChangeHealth(-addedTempHealth);
			}
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override bool Validate() {
			return base.Validate() && buffOwner is IPlayerEntity;
		}


		protected override void OnLevelUp() {
			this.MaxDuration = GetBuffPropertyAtCurrentLevel<float>("time");
			this.RemainingDuration = MaxDuration;
			RecoverSpeed();
			AddSpeed();
		}

		protected override void OnBuffStacked(StimulatedBuff buff) {
			this.MaxDuration = Mathf.Max(buff.MaxDuration, this.MaxDuration);
			this.RemainingDuration = MaxDuration;
		}

		private void AddSpeed() {
			IWalkSpeed walkSpeed = playerEntity.GetWalkSpeed();
			ISprintSpeed sprintSpeed = playerEntity.GetSprintSpeed();
			ISlideSpeed slideSpeed = playerEntity.GetSlideSpeed();
				
			AddSpeed(walkSpeed, ref addedWalkSpeed);
			AddSpeed(sprintSpeed, ref addedSprintSpeed);
			AddSpeed(slideSpeed, ref addedSlideSpeed);
		}
		
		private void AddSpeed(IProperty<float> property, ref float addedValue) {
			float baseValue = property.BaseValue;
			float multiplayer = GetBuffPropertyAtCurrentLevel<float>("move_speed");
			float increasedValue = baseValue * multiplayer;
			property.RealValue.Value += increasedValue;
			addedValue = increasedValue;
		}
		
		private void RecoverSpeed() {
			if (playerEntity == null) {
				return;
			}

			IWalkSpeed walkSpeed = playerEntity.GetWalkSpeed();
			ISprintSpeed sprintSpeed = playerEntity.GetSprintSpeed();
			ISlideSpeed slideSpeed = playerEntity.GetSlideSpeed();
			
			walkSpeed.RealValue.Value -= addedWalkSpeed;
			sprintSpeed.RealValue.Value -= addedSprintSpeed;
			slideSpeed.RealValue.Value -= addedSlideSpeed;
			
			addedWalkSpeed = 0;
			addedSprintSpeed = 0;
			addedSlideSpeed = 0;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			if (playerEntity != null) {
				playerEntity.UnregisterOnModifyDamageCount(OnPlayerModifyDamageCount);
				playerEntity.HealthProperty.RealValue.UnRegisterOnValueChanged(OnPlayerHealthChanged);
			}
			addedTempHealth = 0;
		}
	}
}