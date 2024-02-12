using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant {
	public class PlantBuff : WeaponBuildBuff<PlantBuff>, ICanGetSystem {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			weaponEntity.RegisterOnDealDamage(OnDealDamage);
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		private HitData OnModifyHitData(HitData data, IWeaponEntity weapon) {
			if (data.Hurtbox == null) {
				return data;
			}
			
			if (Level < 2) {
				return data;
			}
			
			if (data.Hurtbox.Owner &&
			    data.Hurtbox.Owner.TryGetComponent<IEntityViewController>(out IEntityViewController vc)) {

				IEntity entity = vc.Entity;
				IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();

				if (buffSystem.ContainsBuff<HackedBuff>(entity, out IBuff buff)) {
					float hit_buff_multiplier = GetBuffPropertyAtCurrentLevel<float>("hit_buff_multiplier");
					data.Damage = Mathf.RoundToInt(data.Damage * (1 + hit_buff_multiplier));
				}
			}

			return data;
		}

		private void OnDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			if (Random.Range(0f, 1f) <= chance) {
				Transform transform = weaponEntity.GetRootDamageDealerTransform();
				if (!transform) {
					return;
				}
			
				
				
				IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
				float duration = GetBuffPropertyAtCurrentLevel<float>("buff_time");
				MalfunctionBuff buff = MalfunctionBuff.Allocate(weaponEntity, target, duration);
				if (!buffSystem.AddBuff(target, weaponEntity, buff)) {
					buff.RecycleToCache();
				}

				if (Level > 2) {
					PowerlessBuff powerlessBuff = PowerlessBuff.Allocate(weaponEntity, target, 2, duration);
					if (!buffSystem.AddBuff(target, weaponEntity, powerlessBuff)) {
						powerlessBuff.RecycleToCache();
					}
				}
				
				float range = GetBuffPropertyAtCurrentLevel<float>("range");
				HashSet<IEnemyEntity> enemies = new HashSet<IEnemyEntity>();
				LayerMask mask = LayerMask.GetMask("Default");
				Collider[] colliders = Physics.OverlapSphere(transform.position, range, mask);
				
				float damageMultiplier = GetBuffPropertyAtCurrentLevel<float>("damage_multiplier");
				int damagePerRarity = GetBuffPropertyAtCurrentLevel<int>("damage_per_rarity");
				
				foreach (var collider in colliders) {
					if(!collider.attachedRigidbody) continue;
					IEnemyViewController enemy = collider.attachedRigidbody.GetComponent<IEnemyViewController>();
					if (enemy != null && enemy.EnemyEntity != null) {
						enemies.Add(enemy.EnemyEntity);
					}
				}
						
				//apply buff to all enemies
				foreach (var enemy in enemies) {
					if (enemy != target) {
						enemy.TakeDamage(Mathf.RoundToInt(damagePerRarity * weaponEntity.GetRarity() * damageMultiplier), weaponEntity);
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
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		protected override bool OnValidate() {
			return true;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			if (weaponEntity != null) {
				weaponEntity.UnregisterOnDealDamage(OnDealDamage);
				weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
			}
			base.OnRecycled();
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}