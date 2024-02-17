using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Mineral;
using Cysharp.Threading.Tasks;
using Polyglot;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Magazines.DoubleExplosionMagazine {
	public class DoubleExplosionMagazine : WeaponPartsEntity<DoubleExplosionMagazine, DoubleExplosionMagazineBuff> {
		public override string EntityName { get; set; } = "DoubleExplosionMagazine";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float multiplayer = GetCustomDataValueOfCurrentLevel<float>("multiplier");
			string displayMultiplayer = multiplayer.ToString("f2");
			return Localization.GetFormat(defaultLocalizationKey, displayMultiplayer);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class DoubleExplosionMagazineBuff : WeaponPartsBuff<DoubleExplosionMagazine, DoubleExplosionMagazineBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
	
		public override void OnInitialize() {
			
			RegisterWeaponBuildBuffEvent<OnCombatBuffGenerateExplostion>(OnCombatBuffGenerateExplostion);

		}

		private void OnCombatBuffGenerateExplostion(OnCombatBuffGenerateExplostion e) {
			OnDoubleExplosion(e);
		}
		
		private async UniTask OnDoubleExplosion(OnCombatBuffGenerateExplostion e) {
			string uuid = weaponEntity.UUID + weaponPartsEntity.UUID;
			IEntityViewController target = e.HurtboxOwner.GetComponent<IEntityViewController>();
			
			string targetUUID = target?.Entity?.UUID;
			
			await UniTask.WaitForSeconds(0.3f);
			if(weaponEntity.UUID + weaponPartsEntity.UUID != uuid) {
				return;
			}

			if (e.Buff.IsRecycled || !e.HurtboxOwner) {
				return;
			}
			
			target = e.HurtboxOwner.GetComponent<IEntityViewController>();
			if(target.Entity == null || targetUUID != target.Entity.UUID || target.Entity.UUID == null) {
				return;
			}
			
			float multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
			int damage = Mathf.RoundToInt(e.Damage * multiplier);
			e.Buff.GenerateExplosion(damage, e.HurtboxOwner, e.HitPoint, e.Attacker);
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
			UnRegisterWeaponBuildBuffEvent<OnCombatBuffGenerateExplostion>(OnCombatBuffGenerateExplostion);
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					float multiplier = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("multiplier");
					string multiplierString = multiplier.ToString("f2");

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("DoubleExplosionMagazine_desc", multiplierString));
				})
			};
		}
	}
}