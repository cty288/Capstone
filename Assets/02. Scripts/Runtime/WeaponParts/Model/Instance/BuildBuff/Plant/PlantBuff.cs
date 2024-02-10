﻿using System;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant {


	public class PlantBuffMultiplierEvent : ModifyValueEvent<float> {
		public PlantBuffMultiplierEvent(float value) : base(value) {
		}
	}

	public class PlantAOEEvent : WeaponBuildBuffEvent {
		public PlantBuff Buff { get; set; }
		public float Duration { get; set; }
		public IDamageable Target { get; set; }
	}
	
	public class PlantAOEKillEvent : WeaponBuildBuffEvent {
		public PlantBuff Buff { get; set; }
		public IDamageable Target { get; set; }
		public Transform Transform { get; set; }
	}
	
	public class PlantBuffAOE : ICanDealDamage {
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>();
		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			 
		}

		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

		Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}

		public ICanDealDamage ParentDamageDealer { get; }
		
		public PlantBuffAOE(ICanDealDamage parentDamageDealer) {
			ParentDamageDealer = parentDamageDealer;
			CurrentFaction.Value = parentDamageDealer.CurrentFaction.Value;
		}
	}
	
	public class PlantBuff : WeaponBuildBuff<PlantBuff>, ICanGetSystem {
		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			
			weaponEntity.RegisterOnModifyHitData(OnWeaponModifyHitData);
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		

		public override string[] GetAllLevelDescriptions() {
			int displayedChance = Mathf.RoundToInt(GetBuffPropertyAtLevel<float>("chance", 1) * 100);
			int time = Mathf.RoundToInt(GetBuffPropertyAtLevel<float>("buff_time", 1));
			int additionalDamage = weaponEntity.GetRarity() * GetBuffPropertyAtLevel<int>("damage_per_rarity", 1);
			string malfunctionBuffDesc = Localization.Get("MulfunctionBuff_Desc");

			int displayedPercentage = Mathf.RoundToInt(GetBuffPropertyAtLevel<float>("hit_buff_multiplier", 2) * 100);
			
			PowerlessBuff powerlessBuff =
				BuffPool.GetTemplateBuffs((buff => buff is PowerlessBuff)).FirstOrDefault() as PowerlessBuff;
			string powerlessBuffDesc = powerlessBuff?.GetLevelDescription(2);
			
			
			return new string[] {
                Localization.GetFormat("BUILD_BUFF_PLANT_1", displayedChance, time, additionalDamage, malfunctionBuffDesc),
                Localization.GetFormat("BUILD_BUFF_PLANT_2", displayedPercentage),
                Localization.GetFormat("BUILD_BUFF_PLANT_3", powerlessBuffDesc)
			};

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

				if (buffSystem.ContainsBuff<MalfunctionBuff>(entity, out IBuff buff)) {
					float hit_buff_multiplier = GetBuffPropertyAtCurrentLevel<float>("hit_buff_multiplier");
					data.Damage = Mathf.RoundToInt(data.Damage * (1 + hit_buff_multiplier));
				}
			}

			return data;
		}

		
		/// <summary>
		/// Will also add powerless buff if level is greater than 2
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="target"></param>
		public void AddMulfunctionBuff(float duration, IDamageable target) {
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
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
		}

		public void RangeAOE(float range, int damage, float duration, Transform transform, IDamageable target, 
			ICanDealDamage damageDealer, bool sendBuildBuffEvent) {
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
					if (sendBuildBuffEvent) {
						SendWeaponBuildBuffEvent(new PlantAOEEvent() {
							Buff = this,
							Duration = duration,
							Target = enemy
						});
					}

					enemy.TakeDamage(Mathf.RoundToInt(damage), damageDealer, out bool isDie);
					if (isDie && sendBuildBuffEvent) {
						SendWeaponBuildBuffEvent(new PlantAOEKillEvent() {
							Buff = this,
							Target = enemy,
							Transform = transform
						});
					}
				}
			}
		}

		private HitData OnWeaponModifyHitData(HitData hit, IWeaponEntity weaponEntity) {
			float chance = GetBuffPropertyAtCurrentLevel<float>("chance");
			if (Random.Range(0f, 1f) <= chance) {
				Transform transform = weaponEntity.GetRootDamageDealerTransform();
				if (!transform) {
					return hit;
				}

				if (hit.Hurtbox == null || !hit.Hurtbox.Owner) {
					return hit;
				}
				
				IDamageable target = hit.Hurtbox.Owner.GetComponent<IDamageableViewController>()?.DamageableEntity;
				if (target == null) {
					return hit;
				}

				float duration = GetBuffPropertyAtCurrentLevel<float>("buff_time");
				AddMulfunctionBuff(duration, target);

				
				float range = GetBuffPropertyAtCurrentLevel<float>("range");
				int damagePerRarity = GetBuffPropertyAtCurrentLevel<int>("damage_per_rarity");
				float damageMultiplier = GetBuffPropertyAtCurrentLevel<float>("damage_multiplier");
				damageMultiplier = weaponEntity.SendModifyValueEvent(new PlantBuffMultiplierEvent(damageMultiplier))
					.Value;
				int realDamage = Mathf.RoundToInt(damagePerRarity * weaponEntity.GetRarity() * damageMultiplier);


				RangeAOE(range, realDamage, duration, hit.Hurtbox.Owner.transform, target, new PlantBuffAOE(weaponEntity), true);
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

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		protected override bool OnValidate() {
			return true;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			if (weaponEntity != null) {
				weaponEntity.UnRegisterOnModifyHitData(OnWeaponModifyHitData);
				weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
			}
			base.OnRecycled();
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}