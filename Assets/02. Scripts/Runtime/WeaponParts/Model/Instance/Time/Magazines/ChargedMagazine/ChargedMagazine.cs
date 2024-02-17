using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Time.Magazines.ChargedMagazine {
	public class ChargedMagazine : WeaponPartsEntity<ChargedMagazine, ChargedMagazineBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "ChargedMagazine";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int condition = GetCustomDataValueOfCurrentLevel<int>("condition");
			int effect = GetCustomDataValueOfCurrentLevel<int>("effect");
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplayer");

			int displayMultiplayer = (int) (multiplayer * 100);

			return Localization.GetFormat(defaultLocalizationKey, condition, effect, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class ChargedMagazineBuff : WeaponPartsBuff<ChargedMagazine, ChargedMagazineBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;


		[ES3Serializable] private int remainingAmmoToTrigger = 0;
		[ES3Serializable] private int remainingAmmoToEndEffect = 0;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
			weaponEntity.RegisterOnUseAmmo(OnUseAmmo);
		}

		private void OnUseAmmo(int ammoCount) {
			
		}

		private HitData OnModifyHitData(HitData hit, IWeaponEntity weapon) {
			if (hit.Hurtbox == null || !hit.Hurtbox.Owner) {
				return hit;
			}

			IEnemyViewController enemyViewController = hit.Hurtbox.Owner.GetComponent<IEnemyViewController>();
			if (enemyViewController == null) {
				return hit;
			}

			IEnemyEntity enemyEntity = enemyViewController.EnemyEntity;
			if (enemyEntity == null) {
				return hit;
			}

			if (remainingAmmoToTrigger > 0) {
				remainingAmmoToTrigger--;
			}else {
				float multiplayer = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplayer");
				hit.Damage = Mathf.RoundToInt(hit.Damage * (1 + multiplayer));
				remainingAmmoToEndEffect--;

				if (remainingAmmoToEndEffect <= 0) {
					remainingAmmoToTrigger = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("condition");
					remainingAmmoToEndEffect = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("effect");
				}
			}

			Debug.Log("Remaining Ammo to trigger: " + remainingAmmoToTrigger + " Remaining Ammo to end effect: " +
			          remainingAmmoToEndEffect);
			
			return hit;
		}


		public override void OnStart() {
			base.OnStart();
			remainingAmmoToTrigger = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("condition");
			remainingAmmoToEndEffect = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("effect");
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
			weaponEntity.UnRegisterOnUseAmmo(OnUseAmmo);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {

					int condition = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("condition");
					int effect = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("effect");
					float multiplayer = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplayer");
					
					int displayMultiplayer = (int) (multiplayer * 100);


					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("ChargedMagazine_desc", condition, effect, displayMultiplayer));
				})
			};
		}
	}
}