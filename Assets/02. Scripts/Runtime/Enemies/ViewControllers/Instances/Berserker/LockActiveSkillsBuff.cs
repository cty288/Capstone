using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;

namespace Runtime.Enemies.ViewControllers.Instances.Berserker {
	public class LockActiveSkillsBuff : Buff<LockActiveSkillsBuff>, ICanGetSystem, ICanGetModel {
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = -1;

		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;
		[field: ES3Serializable] public override int Priority { get; } = 1;

		[field: ES3Serializable] private HashSet<string> lockedSkillEntityID = new HashSet<string>();
		
 		public override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		public override bool IsDisplayed() {
			return true;
		}

		public override bool Validate() {
			return buffOwner is IPlayerEntity;
		}

		public override void OnInitialize() {
			
		}

		public override bool OnStacked(LockActiveSkillsBuff buff) {
			return false;
		}

		public override void OnStart() {
			IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
			var slots = inventoryModel.GetAllSlots((slot => true));
			foreach (var slot in slots) {
				string uuid = slot.GetLastItemUUID();
				if(String.IsNullOrEmpty(uuid)) continue;
				ISkillEntity skillEntity = GlobalGameResourceEntities.GetAnyResource(uuid) as ISkillEntity;
				if (skillEntity == null || skillEntity is IPassiveSkillEntity) continue;
				skillEntity.AdditionalSkillSwitchLocker.Retain();
				lockedSkillEntityID.Add(skillEntity.UUID);
			}
		}

		private void OnBuffUpdate(IBuff buff, BuffUpdateEventType updateType) {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		[field: ES3Serializable]
		public override bool IsGoodBuff { get; } = false;
		public override void OnEnds() {
			foreach (var skillEntityID in lockedSkillEntityID) {
				ISkillEntity skillEntity = GlobalGameResourceEntities.GetAnyResource(skillEntityID) as ISkillEntity;
				if (skillEntity == null) continue;
				skillEntity.AdditionalSkillSwitchLocker.Release();
			}
		}

		public override void OnRecycled() {
			base.OnRecycled();
			lockedSkillEntityID.Clear();
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
		
		/*public static LockActiveSkillsBuff Allocate(IEntity buffDealer, IEntity buffOwner) {
			LockActiveSkillsBuff buff = LockActiveSkillsBuff.Allocate(buffDealer, buffOwner);
			return buff;
		}*/
	}
}