using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Weapons.Model.Properties;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Attachments {
	public class VerticalForegrip : WeaponPartsEntity<VerticalForegrip,VerticalForegripBuff> {
		public override string EntityName { get; set; } = "VerticalForegrip";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int displayMultiplayer = (int) (multiplayer * 100);
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer);
		}

		public override int GetMaxRarity() {
			return 1;
		}
		
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class VerticalForegripBuff : WeaponPartsBuff<VerticalForegrip, VerticalForegripBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		private BuffedProperties<RecoilInfo> baseRecollProperties;


		[ES3Serializable] private RecoilInfo addedRecoil;
		
		
		public override void OnInitialize() {
			baseRecollProperties = new BuffedProperties<RecoilInfo>(weaponEntity, true, BuffTag.Weapon_HipFireRecoil);
		}

		public override void OnStart() {
			base.OnStart();
			float multiplayer = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
			IBuffedProperty<RecoilInfo> recoilProperty = baseRecollProperties.Properties.First();

			RecoilInfo recoilInfo = recoilProperty.BaseValue;

			addedRecoil = new RecoilInfo(-recoilInfo.RecoilX * multiplayer, -recoilInfo.RecoilY * multiplayer,
				-recoilInfo.RecoilZ * multiplayer, 0, 0);
			
			recoilProperty.RealValue.Value += addedRecoil;
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			IBuffedProperty<RecoilInfo> recoilProperty = baseRecollProperties.Properties.First();
			recoilProperty.RealValue.Value -= addedRecoil;
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return new[] {baseRecollProperties};
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					float multiplayer = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
					int displayMultiplayer = (int) (multiplayer * 100);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("VerticalForegrip_desc", displayMultiplayer));
				})
			};
		}
	}
}