using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel {
	public class HeavyBarrel : WeaponPartsEntity<HeavyBarrel, HeavyBarrelPartsBuff> {
		public override string EntityName { get; set; } = "HeavyBarrel";
		public float BuffChance => GetCustomDataValueOfCurrentLevel<float>("chance");
		public int BuffLevel => GetCustomDataValueOfCurrentLevel<int>("buff_level");
		

		public string GetBleedingBuffDescription() {
			int buffLevel = BuffLevel;
			float damage  = BleedingBuff.GetBuffPropertyAtLevel<float>("BleedingBuff", "buff_damage", buffLevel);
			float duration = BleedingBuff.GetBuffPropertyAtLevel<float>("BleedingBuff", "buff_length", buffLevel);
			
			
			if (buffLevel <= 2) {
				return Localization.GetFormat($"BUFF_BLEEDING_{buffLevel}", damage, duration);
			}
			else {
				float buffDamage = damage * 100;
				return Localization.GetFormat($"BUFF_BLEEDING_{buffLevel}", buffDamage.ToString("f2"),
					duration);
			}
		}
		
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int chance = (int) (BuffChance * 100);
			int buffLevel = BuffLevel;
			return Localization.GetFormat(defaultLocalizationKey, chance + "%", buffLevel, GetBleedingBuffDescription());
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Barrel;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class HeavyBarrelPartsBuff : WeaponPartsBuff<HeavyBarrel, HeavyBarrelPartsBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		
		private IBuffSystem buffSystem;
		public override void OnInitialize() {
			weaponEntity.RegisterOnDealDamage(OnWeaponDealDamage);
			
		}

		private void OnWeaponDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			buffSystem = this.GetSystem<IBuffSystem>();
			float chance = weaponPartsEntity.BuffChance;
			if (Random.Range(0f, 1f) > chance) {
				return;
			}

			IEntity rootEntity = weaponEntity.GetRootDamageDealer() as IEntity;
			buffSystem.AddBuff(target,rootEntity, BleedingBuff.Allocate(
				1, weaponPartsEntity.BuffLevel, rootEntity, target));
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			weaponEntity.UnregisterOnDealDamage(OnWeaponDealDamage);
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int chance = (int) (weaponPartsEntity.BuffChance * 100);
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("HeavyBarrel_BUFF_PROPERTY", chance + "%",
							weaponPartsEntity.BuffLevel));
				})
			};
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}