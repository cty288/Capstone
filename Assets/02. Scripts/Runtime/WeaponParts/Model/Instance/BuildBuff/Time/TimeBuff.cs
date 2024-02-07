using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Player;
using Runtime.Weapons.Model.Properties;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Time {
	public class TimeBuff : WeaponBuildBuff<TimeBuff>, ICanGetSystem {
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = 0.1f;
		private IPlayerEntity playerEntity = null;
		[ES3Serializable]
		private float reloadSpeedSubtraction = 0;
		
		[field: ES3Serializable]
		private float timer = 0;
		
		private float levelUpTime = 0;
		private IBuffSystem buffSystem = null;
		public override void OnInitialize() {
			if (weaponEntity == null) {
				return;
			}
			
			
			IEntity weaponRootOwner = weaponEntity.GetRootDamageDealer() as IEntity;
			if (weaponRootOwner == null || weaponRootOwner is not IPlayerEntity) {
				return;
			}
			
			
			weaponEntity.RegisterOnDealDamage(OnDealDamage);
			playerEntity = weaponRootOwner as IPlayerEntity;
			playerEntity.RegisterOnBuffUpdate(OnBuffUpdate);
			levelUpTime = GetBuffPropertyAtLevel<float>("level_up_time", 3);
		}

		private void OnBuffUpdate(IBuff buff, BuffUpdateEventType updateType) {
			if(buff is not MotivatedBuff motivatedBuff) {
				return;
			}
			
			if(Level < 2) {
				return;
			}
			
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();

			IReloadSpeed reloadSpeed = weaponEntity.GetReloadSpeed();
			float subtraction = GetBuffPropertyAtCurrentLevel<float>("reload");
			if (updateType == BuffUpdateEventType.OnStart && reloadSpeedSubtraction <= 0) {
				reloadSpeedSubtraction = subtraction;
				if (reloadSpeedSubtraction > reloadSpeed.RealValue.Value) {
					reloadSpeedSubtraction = Mathf.Max(reloadSpeed.RealValue.Value - 0.05f, 0);
				}
				reloadSpeed.RealValue.Value -= reloadSpeedSubtraction;
				
			}else if (updateType == BuffUpdateEventType.OnEnd) {
				reloadSpeed.RealValue.Value += reloadSpeedSubtraction;
				reloadSpeedSubtraction = 0;
			}
		}

		private void OnDealDamage(ICanDealDamage source, IDamageable target, int damage) {
			IEntity weaponRootOwner = weaponEntity.GetRootDamageDealer() as IEntity;
			if (weaponRootOwner == null) {
				return;
			}
			
			
			MotivatedBuff motivatedBuff = MotivatedBuff.Allocate(weaponRootOwner,
				weaponRootOwner, 1);
			
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
			Debug.Log("TimeBuff.OnDealDamage");
			buffSystem.AddBuff
			(weaponRootOwner, weaponRootOwner, motivatedBuff);
		}

		public override void OnStart() {
			
		}

		public override BuffStatus OnTick() {
			if (Level < 3 || playerEntity == null || !weaponEntity.IsHolding) {
				return BuffStatus.Running;
			}

			if (buffSystem == null) {
				buffSystem = this.GetSystem<IBuffSystem>();
			}


			if (buffSystem.ContainsBuff<MotivatedBuff>(playerEntity, out IBuff buff)) {
				timer += TickInterval;
				if (timer >= levelUpTime) {
					MotivatedBuff motivatedBuff = buff as MotivatedBuff;
					if (motivatedBuff == null) {
						return BuffStatus.Running;
					}
					
					motivatedBuff.LevelUp();
					timer = 0;
				}
			}
			else {
				timer = 0;
			}
			
			
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			if (reloadSpeedSubtraction > 0) {
				IReloadSpeed reloadSpeed = weaponEntity.GetReloadSpeed();
				reloadSpeed.RealValue.Value += reloadSpeedSubtraction;
				reloadSpeedSubtraction = 0;
			}
			
			reloadSpeedSubtraction = 0;
		}

		public override void OnRecycled() {
			if (weaponEntity != null) {
				weaponEntity.UnregisterOnDealDamage(OnDealDamage);
			}
			
			if (playerEntity != null) {
				playerEntity.UnregisterOnBuffUpdate(OnBuffUpdate);
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