using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff {
	public class OnPlantBuffChangeDOTEvent : ModifyValueEvent<int> {
		public OnPlantBuffChangeDOTEvent(int value) : base(value) {
		}
	}
	
	public class OnPlantBuffStackBuffEvent : WeaponBuildBuffEvent {
		public IDamageable Target { get; set; }
		public HackedBuff Buff { get; set; }
	}
	
	
	public class OnPlantBuffGenerateInitialBuffEvent : WeaponBuildBuffEvent {
		public IDamageable Target { get; set; }
	}
	
	public class OnPlantBuffModifyChanceEvent : ModifyValueEvent<float> {
		public OnPlantBuffModifyChanceEvent(float value) : base(value) {
		}
	}
	
	
	
	public class OnPlantBuffChangeBaseDurationEvent : ModifyValueEvent<float> {
		public OnPlantBuffChangeBaseDurationEvent(float value) : base(value) {
		}
	}
	
	public class OnPlantBuffChangeTotalDurationEvent : ModifyValueEvent<float> {
		public OnPlantBuffChangeTotalDurationEvent(float value) : base(value) {
		}
	}
	
	public class OnPlantBuffChangeTotalDamageEvent : ModifyValueEvent<float> {
		public OnPlantBuffChangeTotalDamageEvent(float value) : base(value) {
		}
	}
	
	public class OnPlantBuffChangeIsSuddenDeathEvent : ModifyValueEvent<bool> {
		public OnPlantBuffChangeIsSuddenDeathEvent(bool value) : base(value) {
		}
	}

	public class OnHackedBuffAdded : WeaponBuildBuffEvent {
		public IDamageable Target { get; set; }
		public Transform TargetTransform { get; set; }
		
		
	}
	public class PlantBuff : WeaponBuildBuff<PlantBuff>, ICanGetSystem {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = -1;

		//private Collider[] results = new Collider[];
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		


		public bool AddHackedBuff(Transform targetTransform, IDamageable target) {
			float damageMultiplier = GetBuffPropertyAtCurrentLevel<float>("damage_multiplier");
			float baseDuration = GetBuffPropertyAtCurrentLevel<float>("time");
			baseDuration = weaponEntity
				.SendModifyValueEvent<OnPlantBuffChangeBaseDurationEvent>(
					new OnPlantBuffChangeBaseDurationEvent(baseDuration))
				.Value;

			float actualDuration = weaponEntity
				.SendModifyValueEvent<OnPlantBuffChangeTotalDurationEvent>(
					new OnPlantBuffChangeTotalDurationEvent(baseDuration))
				.Value;
			
			int damagePerSec = 2 * weaponEntity.GetRarity();
			damagePerSec = weaponEntity.SendModifyValueEvent<OnPlantBuffChangeDOTEvent>(new OnPlantBuffChangeDOTEvent(damagePerSec))
				.Value;


			float totalDamage = Mathf.RoundToInt(damagePerSec * baseDuration);
			totalDamage = weaponEntity.SendModifyValueEvent<OnPlantBuffChangeTotalDamageEvent>(
				new OnPlantBuffChangeTotalDamageEvent(totalDamage)).Value;
			int actualDamagePerSec = Mathf.CeilToInt((totalDamage / actualDuration) - 0.01f);

			bool isSuddenDeath = false;
			isSuddenDeath = weaponEntity.SendModifyValueEvent<OnPlantBuffChangeIsSuddenDeathEvent>(
				new OnPlantBuffChangeIsSuddenDeathEvent(isSuddenDeath)).Value;

			return AddHackedBuff(weaponEntity, targetTransform, target, actualDuration, actualDamagePerSec, damageMultiplier,
				isSuddenDeath,
				(int) totalDamage);

		}

		public static bool AddHackedBuff(IWeaponEntity weaponEntity, Transform targetTransform, IDamageable target, float actualDuration, int actualDamagePerSec,
			float damageMultiplier, bool isSuddenDeath, int totalDamage) {
			IBuffSystem buffSystem = MainGame.Interface.GetSystem<IBuffSystem>();
			
			HackedBuff buff = HackedBuff.Allocate(weaponEntity, target, actualDuration, actualDamagePerSec,
				damageMultiplier, isSuddenDeath, (int) totalDamage);
			if (buffSystem.AddBuff(target, weaponEntity, buff)) {
				SendWeaponBuildBuffEvent<OnHackedBuffAdded>(weaponEntity, new OnHackedBuffAdded()
					{Target = target, TargetTransform = targetTransform});
				return true;
			}
			else {
				//stack failed
				if(buffSystem.ContainsBuff<HackedBuff>(target, out var hackedBuff)) {
					SendWeaponBuildBuffEvent<OnPlantBuffStackBuffEvent>(weaponEntity, new OnPlantBuffStackBuffEvent() {
						Target = target,
						Buff = hackedBuff as HackedBuff
					});
				}
				buff.RecycleToCache();
				return false;
			}
		}
		
		
		private HitData OnModifyHitData(HitData hit, IWeaponEntity weaponEntity) {
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			chance = weaponEntity
				.SendModifyValueEvent<OnPlantBuffModifyChanceEvent>(new OnPlantBuffModifyChanceEvent(chance)).Value;
			
			
			if(Random.Range(0f, 1f) <= chance) {
				
				
				

				if (hit.Hurtbox == null || !hit.Hurtbox.Owner) {
					return hit;
				}

				Transform transform = hit.Hurtbox.Owner.transform;
				if (!transform) {
					return hit;
				}
				
				
				IDamageable target = hit.Hurtbox.Owner.GetComponent<IDamageableViewController>()?.DamageableEntity;
				if (target == null) {
					return hit;
				}
				
				
				
				if (AddHackedBuff(transform, target)) {
					SendWeaponBuildBuffEvent<OnPlantBuffGenerateInitialBuffEvent>(
						new OnPlantBuffGenerateInitialBuffEvent() {
							Target = target
						});
					
					if (Level >= 2) {
						
						
						float range = GetBuffPropertyAtCurrentLevel<float>("range");
						//get all enemies in range
						Dictionary<IEnemyEntity, Transform> enemies = new Dictionary<IEnemyEntity, Transform>();
						LayerMask mask = LayerMask.GetMask("Default");
						Collider[] colliders = Physics.OverlapSphere(transform.position, range, mask);
						foreach (var collider in colliders) {
							if(!collider.attachedRigidbody) continue;
							IEnemyViewController enemy = collider.attachedRigidbody.GetComponent<IEnemyViewController>();
							if (enemy != null && enemy.EnemyEntity != null) {
								enemies.TryAdd(enemy.EnemyEntity, collider.transform);
							}
						}
						
						//apply buff to all enemies
						foreach (var enemyKVP in enemies) {
							if (enemyKVP.Key != target) {
								AddHackedBuff(enemyKVP.Value, enemyKVP.Key);
							}
						}
					}
				}
			}
			
			return hit;
		}
		private void OnDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			
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
				weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
			}
			base.OnRecycled();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		protected override bool OnValidate() {
			return true;
		}

		public override string[] GetAllLevelDescriptions() {
			int displayedChance = Mathf.RoundToInt(GetBuffPropertyAtCurrentLevel<float>("chance") * 100);
			int time = Mathf.RoundToInt(GetBuffPropertyAtCurrentLevel<float>("time"));
			int calculatedDamage = 2 * weaponEntity.GetRarity();
			
			return new string[] {
				Localization.GetFormat("BUILD_BUFF_PLANT_1", displayedChance, time, calculatedDamage),
				Localization.Get("BUILD_BUFF_PLANT_2"),
				Localization.Get("BUILD_BUFF_PLANT_3")
			};
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}