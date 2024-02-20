using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Weapons.Model.Properties;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Attachments.KillCounter {
	public class KillCounter : WeaponPartsEntity<KillCounter,KillCounterBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "KillCounter";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int counter = GetCustomDataValueOfCurrentLevel<int>("counter");
			int damage = GetCustomDataValueOfCurrentLevel<int>("damage");
			return Localization.GetFormat(defaultLocalizationKey, counter, damage);
		}

		public override int GetMaxRarity() {
			return 3;
		}
		
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class KillCounterBuff : WeaponPartsBuff<KillCounter, KillCounterBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		private BuffedProperties<Vector2Int> baseDamageProperties;
		[ES3Serializable]
		private int counter = 0;
		
		[ES3Serializable]
		private int addedDamage = 0;
		
		public override void OnInitialize() {
			baseDamageProperties = new BuffedProperties<Vector2Int>(weaponEntity, true, BuffTag.Weapon_BaseDamage);
			weaponEntity.RegisterOnKillDamageable(OnKillDamageable);
		}

		public override void OnAwake() {
			base.OnAwake();
			if (addedDamage > 0) {
				IBuffedProperty<Vector2Int> baseDamageProperty = baseDamageProperties.Properties.First();
				baseDamageProperty.RealValue.Value += new Vector2Int(addedDamage, addedDamage);
			}
		}

		private void OnKillDamageable(ICanDealDamage source, IDamageable target) {
			if (target is IEnemyEntity enemy) {
				int targetCounter = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("counter");
				counter++;
				
				if (counter >= targetCounter) {
					counter = 0;
					int addedDamage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
					this.addedDamage += addedDamage;
					IBuffedProperty<Vector2Int> baseDamageProperty = baseDamageProperties.Properties.First();
					baseDamageProperty.RealValue.Value += new Vector2Int(addedDamage, addedDamage);
				}
			}
		}

		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			IBuffedProperty<Vector2Int> baseAttackSpeedProperty = baseDamageProperties.Properties.First();
			baseAttackSpeedProperty.RealValue.Value -= new Vector2Int(addedDamage, addedDamage);
		}


		public override void OnRecycled() {
			weaponEntity.UnregisterOnKillDamageable(OnKillDamageable);
			base.OnRecycled();
			counter = 0;
			addedDamage = 0;
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return new[] {baseDamageProperties};
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					int counter = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("counter");
					int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
					string partsName = weaponPartsEntity.GetDisplayName();

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("KillCounter_PROPERTY_desc", counter, damage, partsName));
				})
			};
		}
	}
}