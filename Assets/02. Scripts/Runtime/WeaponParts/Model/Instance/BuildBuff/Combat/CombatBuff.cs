using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat {
	public class CombatBuff : WeaponBuildBuff<CombatBuff>, ICanGetSystem {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = -1;

		//private Collider[] results = new Collider[];
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			weaponEntity.RegisterOnDealDamage(OnDealDamage);
		}

		private void OnDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			if(Random.Range(0f, 1f) <= chance) {
				float damageMultiplier = GetBuffPropertyAtCurrentLevel<float>("damage_multiplier");
				float duration = GetBuffPropertyAtCurrentLevel<float>("time");
				
				
				IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
				HackedBuff buff = HackedBuff.Allocate(weaponEntity, target, duration, 2 * weaponEntity.GetRarity(),
					damageMultiplier);

				if (buffSystem.AddBuff(target, weaponEntity, buff)) {
					if (Level >= 2) {
						
						Transform transform = weaponEntity.GetRootDamageDealerTransform();
						if (!transform) {
							return;
						}
						float range = GetBuffPropertyAtCurrentLevel<float>("range");
						//get all enemies in range
						HashSet<IEnemyEntity> enemies = new HashSet<IEnemyEntity>();
						LayerMask mask = LayerMask.GetMask("Default");
						/*var size = Physics.OverlapSphereNonAlloc(transform.position, range, results, mask);
						for (int i = 0; i < size; i++) {
							IEnemyViewController enemy = results[i].GetComponent<IEnemyViewController>();
							if (enemy != null && enemy.EnemyEntity != null) {
								enemies.Add(enemy.EnemyEntity);
							}
						}*/
						
						Collider[] colliders = Physics.OverlapSphere(transform.position, range, mask);
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
								HackedBuff aoeBuff = HackedBuff.Allocate(weaponEntity, enemy,
									duration, 2 * weaponEntity.GetRarity(),
									damageMultiplier);

								if (!buffSystem.AddBuff(enemy, weaponEntity, aoeBuff)) {
									aoeBuff.RecycleToCache();
								}
							}
						}
					}
				}else {
					buff.RecycleToCache();
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

		public override void OnRecycled() {
			if (weaponEntity != null) {
				weaponEntity.UnregisterOnDealDamage(OnDealDamage);
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