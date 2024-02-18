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
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff {
	public class OnPlantBuffChangeDOTEvent : ModifyValueEvent<int> {
		public OnPlantBuffChangeDOTEvent(int value) : base(value) {
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
	}
	public class PlantBuff : WeaponBuildBuff<PlantBuff>, ICanGetSystem {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = -1;

		//private Collider[] results = new Collider[];
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			weaponEntity.RegisterOnDealDamage(OnDealDamage);
		}


		public bool AddHackedBuff(IDamageable target) {
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
			
				
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
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

			HackedBuff buff = HackedBuff.Allocate(weaponEntity, target, actualDuration, actualDamagePerSec,
				damageMultiplier, isSuddenDeath, (int) totalDamage);
			

			if (buffSystem.AddBuff(target, weaponEntity, buff)) {
				SendWeaponBuildBuffEvent<OnHackedBuffAdded>(new OnHackedBuffAdded() {Target = target});
				return true;
			}
			else {
				buff.RecycleToCache();
				return false;
			}
		}
		
		
		
		private void OnDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			if(Random.Range(0f, 1f) <= chance) {
				if (AddHackedBuff(target)) {
					if (Level >= 2) {
						
						Transform transform = weaponEntity.GetRootDamageDealerTransform();
						if (!transform) {
							return;
						}
						float range = GetBuffPropertyAtCurrentLevel<float>("range");
						//get all enemies in range
						HashSet<IEnemyEntity> enemies = new HashSet<IEnemyEntity>();
						LayerMask mask = LayerMask.GetMask("Default");

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
								AddHackedBuff(enemy);
							}
						}
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