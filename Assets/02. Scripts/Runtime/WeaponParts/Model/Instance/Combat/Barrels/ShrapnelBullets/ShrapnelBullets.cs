using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Barrels.ShrapnelBullets {
	public class ShrapnelBullets : WeaponPartsEntity<ShrapnelBullets, ShrapnelBulletsBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "ShrapnelBullets";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			int buffLevel = GetCustomDataValueOfCurrentLevel<int>("buff_level");
			VulnerableBuff buff =
				BuffPool.GetTemplateBuffs((b => b is VulnerableBuff)).FirstOrDefault() as VulnerableBuff;

			return Localization.GetFormat(defaultLocalizationKey, buffLevel, buff?.GetLevelDescription(buffLevel));
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class ShrapnelBulletsBuff : WeaponPartsBuff<ShrapnelBullets, ShrapnelBulletsBuff>, ICanGetSystem {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			RegisterWeaponBuildBuffEvent<OnCombatBuffExplosionDealDamge>(OnExplosionDealDamage);
		}

		private void OnExplosionDealDamage(OnCombatBuffExplosionDealDamge e) {
			if(e.IsDie) return;
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();

			BuffBuilder builder =
				BuffPool.FindBuffs((b => b is VulnerableBuff)).FirstOrDefault();
			
			int level = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("buff_level");
			VulnerableBuff buff = builder.Invoke(weaponEntity, e.Target, level) as VulnerableBuff;

			if (!buffSystem.AddBuff(e.Target, weaponEntity, buff)) {
				buff.RecycleToCache();
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

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override void OnRecycled() {
			UnRegisterWeaponBuildBuffEvent<OnCombatBuffExplosionDealDamge>(OnExplosionDealDamage);
			base.OnRecycled();
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {

					int level = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("buff_level");
					string buffDesc =
						(BuffPool.GetTemplateBuffs((b => b is VulnerableBuff)).FirstOrDefault() as VulnerableBuff)
						?.GetLevelDescription(level);

					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("ShrapnelBullets_desc", level, buffDesc));
				})
			};
		}
	}
}