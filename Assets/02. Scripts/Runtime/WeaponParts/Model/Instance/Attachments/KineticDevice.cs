using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Player;
using Runtime.Player.Properties;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Attachments {
public class KineticDevice : WeaponPartsEntity<KineticDevice, KineticDeviceBuff> {
		public override string EntityName { get; set; } = "KineticDevice";
		
		public float multiplayer => GetCustomDataValueOfCurrentLevel<float>("multiplayer");



		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int displayMultiplayer = (int) (multiplayer * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class KineticDeviceBuff : WeaponPartsBuff<KineticDevice, KineticDeviceBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;

		//private float currentIncreasedMovementSpeed;

		/*private Dictionary<IProperty<float>, float>
			increasedSpeedProperties = new Dictionary<IProperty<float>, float>();*/
		
		[field: ES3Serializable]
		private float addedWalkSpeed;
		[field: ES3Serializable]
		private float addedSprintSpeed;
		[field: ES3Serializable]
		private float addedSlideSpeed;

		public override void OnInitialize() {
			weaponEntity.IsHolding.RegisterWithInitValue(OnIsHoldingChanged);
		}

		private void OnIsHoldingChanged(bool arg1, bool isHolding) {
			IPlayerEntity playerEntity = weaponEntity.RootDamageDealer as IPlayerEntity;
			if (playerEntity == null) {
				return;
			}

			
			
			if (isHolding) {
				IWalkSpeed walkSpeed = playerEntity.GetWalkSpeed();
				ISprintSpeed sprintSpeed = playerEntity.GetSprintSpeed();
				ISlideSpeed slideSpeed = playerEntity.GetSlideSpeed();
				
				AddSpeed(walkSpeed, ref addedWalkSpeed);
				AddSpeed(sprintSpeed, ref addedSprintSpeed);
				AddSpeed(slideSpeed, ref addedSlideSpeed);
			}else {
				RecoverSpeed();
			}
		}
		
		private void AddSpeed(IProperty<float> property, ref float addedValue) {
			float baseValue = property.BaseValue;
			float increasedValue = baseValue * weaponPartsEntity.multiplayer;
			property.RealValue.Value += increasedValue;
			addedValue = increasedValue;
		}
		
		private void RecoverSpeed() {
			IPlayerEntity playerEntity = weaponEntity.RootDamageDealer as IPlayerEntity;
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


		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			RecoverSpeed();
			weaponEntity.IsHolding.UnRegisterOnValueChanged(OnIsHoldingChanged);
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters() {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int displayMultiplayer = (int) (weaponPartsEntity.multiplayer * 100);

					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						Localization.GetFormat("KineticDevice_PROPERTY_desc",  displayMultiplayer));
				})
			};
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}

}