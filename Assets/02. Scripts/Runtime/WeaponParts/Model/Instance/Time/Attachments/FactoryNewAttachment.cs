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
public class FactoryNewAttachment : WeaponPartsEntity<FactoryNewAttachment, FactoryNewAttachmentBuff> {
	[field: ES3Serializable]
		public override string EntityName { get; set; } = "FactoryNewAttachment";
		
		public float multiplayer => GetCustomDataValueOfCurrentLevel<float>("multiplayer");
		public float time => GetCustomDataValueOfCurrentLevel<float>("time");
		


		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int displayMultiplayer = (int) (multiplayer * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer, time);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class FactoryNewAttachmentBuff : WeaponPartsBuff<FactoryNewAttachment, FactoryNewAttachmentBuff>, ICanGetSystem {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = 0.1f;

		private float timer;

		
		public override void OnInitialize() {
			weaponEntity.IsHolding.RegisterOnValueChanged(OnIsHoldingChanged);
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		private void OnIsHoldingChanged(bool arg1, bool isHolding) {
			if (isHolding) {
				timer = weaponPartsEntity.time;
			}
			else {
				timer = 0;
			}
		}
		
		

		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			timer -= TickInterval;
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
			timer = 0;
		}

		public override void OnRecycled() {
			weaponEntity.IsHolding.UnRegisterOnValueChanged(OnIsHoldingChanged);
			weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
			base.OnRecycled();
		}


		private HitData OnModifyHitData(HitData hitData, IWeaponEntity weapon) {
			if (timer <= 0) {
				return hitData;
			}

			hitData.Damage = Mathf.RoundToInt(hitData.Damage * (1 + weaponPartsEntity.multiplayer));
			return hitData;
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int displayMultiplayer = (int) (weaponPartsEntity.multiplayer * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("FactoryNewAttachment_PROPERTY_desc", displayMultiplayer,
							weaponPartsEntity.time));
				})
			};
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}

}