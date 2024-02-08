using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
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
			weaponEntity.RegisterOnKillDamageable(OnKillDamageable);
			//weaponEntity.RegisterOnDealDamage();
			
			resLoader = this.GetUtility<ResLoader>();
		}

		private void OnKillDamageable(ICanDealDamage source, IDamageable target) {
			if (Level < 3) {
				return;
			}
			
			if (source is IExplosionViewController) {
				int ammoRecovered = GetBuffPropertyAtCurrentLevel<int>("ammo_recovery_kill");
				weaponEntity.AddAmmo(ammoRecovered);
			}
		}

		private HitData OnWeaponModifyHitData(HitData hit, IWeaponEntity weapon) {
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			if (hit.Attacker is IExplosionViewController) {
				if (Level < 2) return hit;
				
				if (hit.Hurtbox.Owner && hit.Hurtbox.Owner.TryGetComponent<INormalEnemyViewController>(out INormalEnemyViewController vc)) {
					IEnemyEntity enemyEntity = vc.EnemyEntity;
					if(enemyEntity == null) return hit;

					int maxHealth = enemyEntity.GetMaxHealth();
					int currentHealth = enemyEntity.GetCurrentHealth();
					float healthPercentage = (float)currentHealth / maxHealth;
					float killThreshold = GetBuffPropertyAtCurrentLevel<float>("health_kill");

					if (healthPercentage <= killThreshold) {
						enemyEntity.Kill(hit.Attacker, hit);
					}
				}
			}
			else {
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

						if (Level > 2) {
							int ammoRecovered = GetBuffPropertyAtCurrentLevel<int>("ammo_recovery_base");
							weaponEntity.AddAmmo(ammoRecovered);
						}
					}
				
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
				weaponEntity.UnregisterOnKillDamageable(OnKillDamageable);
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