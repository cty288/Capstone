using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Barrels.HardCasesBarrel {
	public class HardCasesBarrel : WeaponPartsEntity<HardCasesBarrel, HardCasesBarrelBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "HardCasesBarrel";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int damage = GetCustomDataValueOfCurrentLevel<int>("damage");
			return Localization.GetFormat(defaultLocalizationKey, damage);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class HardCasesBarrelBuff : WeaponPartsBuff<HardCasesBarrel, HardCasesBarrelBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnDealtBuffUpdate(OnWeaponBuffUpdate);
		}

		private void OnWeaponBuffUpdate(IEntity target, IBuff buff, BuffUpdateEventType eventType) {
			if(buff is not HackedBuff) return;
			if(eventType != BuffUpdateEventType.OnEnd) return;
			if(!weaponEntity.IsHolding) return;
			
			int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
			int calculatedDamage = damage * weaponEntity.GetRarity();
			if (target is IDamageable damageable) {
				damageable.TakeDamage(calculatedDamage, weaponEntity, out _);
			}
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
			weaponEntity.UnRegisterOnDealtBuffUpdate(OnWeaponBuffUpdate);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
					int calculatedDamage = damage * weaponEntity.GetRarity();

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("HardCasesBarrel_desc2", damage, calculatedDamage));
				})
			};
		}
	}
}