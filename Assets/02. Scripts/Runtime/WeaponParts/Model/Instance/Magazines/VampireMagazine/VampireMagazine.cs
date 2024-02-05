using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.Magazines.GunpowerEnhancement;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

public class VampireMagazine : WeaponPartsEntity<VampireMagazine, VampireMagazineBuff> {
		public override string EntityName { get; set; } = "VampireMagazine";
		
		public float Chance => GetCustomDataValueOfCurrentLevel<float>("chance");
		public int Health => GetCustomDataValueOfCurrentLevel<int>("health");

		
	
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int chance = (int) (Chance * 100);
			return Localization.GetFormat(defaultLocalizationKey, chance, Health);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class VampireMagazineBuff : WeaponPartsBuff<VampireMagazine, VampireMagazineBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnDealDamage(OnWeaponDealDamage);
		}

		private void OnWeaponDealDamage(IDamageable target, int damage) {
			if (target is IEnemyEntity) {
				IEntity rootEntity = weaponEntity.GetRootDamageDealer() as IEntity;
				if (rootEntity == null) {
					return;
				}

				if (rootEntity is IDamageable damageable) {
					if (Random.value < weaponPartsEntity.Chance) {
						damageable.Heal(weaponPartsEntity.Health, rootEntity as IBelongToFaction);
					}
				}
			}
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

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters() {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					int chance = (int) (weaponPartsEntity.Chance * 100);
					return new WeaponBuffedAdditionalPropertyDescription(null, null,
						Localization.GetFormat("VampireMagazine_desc", chance, weaponPartsEntity.Health));
				})
			};
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
