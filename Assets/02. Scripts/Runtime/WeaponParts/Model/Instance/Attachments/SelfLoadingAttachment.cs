using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Cysharp.Threading.Tasks;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public class SelfLoadingAttachment : WeaponPartsEntity<SelfLoadingAttachment, SelfLoadingAttachmentBuff> {
		public override string EntityName { get; set; } = "SelfLoadingAttachment";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}


		public override int GetMaxRarity() {
			return 1;
		}

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class SelfLoadingAttachmentBuff : WeaponPartsBuff<SelfLoadingAttachment, SelfLoadingAttachmentBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;

	
		public override void OnInitialize() {
			weaponEntity.IsHolding.RegisterOnValueChanged(OnIsHoldingChanged);
		}

		private void OnIsHoldingChanged(bool arg1, bool isHolding) {
			if (!isHolding) {
				CheckIsHolding();
			}
		}
		
		private async UniTask CheckIsHolding() {
			IWeaponEntity weaponEntity = this.weaponEntity;
			await UniTask.WaitForSeconds(1f);
			if (IsRecycled) return;
			if (weaponEntity == null || weaponEntity.IsHolding == null) return;
			if (weaponEntity != this.weaponEntity) return;
			if(weaponEntity.IsHolding.Value) return;
			
			
			base.weaponEntity.Reload();
		}

		
		public override void OnStart() {
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			weaponEntity.IsHolding.UnRegisterOnValueChanged(OnIsHoldingChanged);
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters() {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						Localization.Get("SelfLoadingAttachment_desc"));
				})
			};
		}
	}
