using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Time;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Player;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Attachments {
	public class BerserkerAttachment : WeaponPartsEntity<BerserkerAttachment, BerserkerAttachmentBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "BerserkerAttachment";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int buffLevel = GetCustomDataValueOfCurrentLevel<int>("buff_level");
			string motivatedBuffDesc = MotivatedBuff.GetDescription(buffLevel, "MotivatedBuff_Desc");
			return Localization.GetFormat(defaultLocalizationKey, buffLevel, motivatedBuffDesc);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class BerserkerAttachmentBuff : WeaponPartsBuff<BerserkerAttachment, BerserkerAttachmentBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			
			IDamageable ownerEntity = weaponEntity.GetRootDamageDealer() as IDamageable;
			if (ownerEntity == null) {
				return;
			}
			
			ownerEntity.HealthProperty.RealValue.RegisterOnValueChanged(OnOwnerHealthChanged);
		}

		private void OnOwnerHealthChanged(HealthInfo oldHealth, HealthInfo newHealth) {
			if (weaponEntity == null || IsRecycled || !weaponEntity.IsHolding) {
				return;
			}
			
			if (newHealth.CurrentHealth < oldHealth.CurrentHealth) {
				int buffLevel = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("buff_level");
				IEntity weaponRootOwner = weaponEntity.GetRootDamageDealer() as IEntity;
				
				MotivatedBuff motivatedBuff = MotivatedBuff.Allocate(weaponRootOwner,
					weaponRootOwner, buffLevel);
			
				IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
				buffSystem.AddBuff
					(weaponRootOwner, weaponRootOwner, motivatedBuff);
			}
		}
		


		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		


		public override void OnRecycled() {
			base.OnRecycled();
			if (weaponEntity == null) {
				return;
			}
			IDamageable ownerEntity = weaponEntity.GetRootDamageDealer() as IDamageable;
			if (ownerEntity == null) {
				return;
			}

			ownerEntity.HealthProperty.RealValue.UnRegisterOnValueChanged(OnOwnerHealthChanged);

		}

		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int buffLevel = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("buff_level");
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("BerserkerAttachment_PROPERTY_desc", buffLevel));
				})
			};
		}
	}
}