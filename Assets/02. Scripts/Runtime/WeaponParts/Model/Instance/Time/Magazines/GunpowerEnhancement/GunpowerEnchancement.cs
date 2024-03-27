using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Magazines.GunpowerEnhancement {
	public class GunpowerEnchancement : WeaponPartsEntity<GunpowerEnchancement, GunpowerEnchancementBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "GunpowerEnchancement";
		

		public string GetBuffDescription() {
			int LayerNumber = DustBuff.GetBuffPropertyAtLevel<int>("DustBuff", "layer_num", GetRarity());
			int Damage = DustBuff.GetBuffPropertyAtLevel<int>("DustBuff", "damage", GetRarity());
			
			return Localization.GetFormat($"BUFF_DUST_desc", LayerNumber, Damage);
		}
		
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return Localization.GetFormat(defaultLocalizationKey, GetRarity(), GetBuffDescription());
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class GunpowerEnchancementBuff : WeaponPartsBuff<GunpowerEnchancement, GunpowerEnchancementBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		
		private IBuffSystem buffSystem;
		public override void OnInitialize() {
			weaponEntity.RegisterOnDealDamage(OnWeaponDealDamage);
		}

		

		private void OnWeaponDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			 buffSystem = this.GetSystem<IBuffSystem>();
			IEntity damageDealer = weaponEntity.GetRootDamageDealer() as IEntity;
			buffSystem.AddBuff(target, damageDealer, DustBuff.Allocate(
				damageDealer, target, weaponPartsEntity.GetRarity()));
		}

		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			weaponEntity.UnregisterOnDealDamage(OnWeaponDealDamage);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						Localization.GetFormat("GunpowerEnhancement_PROPERTY_desc", weaponPartsEntity.GetRarity()));
				})
			};
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}