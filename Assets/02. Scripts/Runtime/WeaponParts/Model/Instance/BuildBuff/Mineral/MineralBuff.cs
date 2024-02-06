using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Mineral {
	public class MineralBuff : WeaponBuildBuff<MineralBuff>, ICanGetUtility {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = -1;

		private ResLoader resLoader;
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			weaponEntity.RegisterOnModifyHitData(OnWeaponModifyHitData);
			
			resLoader = this.GetUtility<ResLoader>();
		}

		private HitData OnWeaponModifyHitData(HitData hit, IWeaponEntity weapon) {
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			if (Random.Range(0f, 1f) <= chance) {
				if (hit.Attacker is not IExplosionViewController) {
					float range = GetBuffPropertyAtCurrentLevel<float>("range");
					float explosionDamagePerRarity = GetBuffPropertyAtCurrentLevel<float>("damage_per_rarity");
					int damage = Mathf.RoundToInt(explosionDamagePerRarity * weaponEntity.GetRarity());
				
					GameObject explosionGo = resLoader.LoadSync<GameObject>("Explosion_MineralBuff");
					MineralBuffExplosion explosion =
						GameObject.Instantiate(explosionGo, hit.HitPoint, Quaternion.identity)
							.GetComponent<MineralBuffExplosion>();

					explosion.Init(weaponEntity.CurrentFaction.Value, damage, range, null, weaponEntity);
				}

			}

			return hit;
		}

		

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		public override void OnRecycled() {
			if (weaponEntity != null) {
				weaponEntity.UnRegisterOnModifyHitData(OnWeaponModifyHitData);
			}
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		protected override bool OnValidate() {
			return true;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}