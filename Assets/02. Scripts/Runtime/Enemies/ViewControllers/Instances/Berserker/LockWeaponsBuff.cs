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
	public class LockWeaponsBuff : Buff<LockWeaponsBuff>, ICanGetSystem, ICanGetModel {
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

		public override bool OnStacked(LockWeaponsBuff buff) {
			return false;
		}

		public override void OnStart() {
			IPlayerEntity playerEntity = buffOwner as IPlayerEntity;
			if (playerEntity == null) return;
			playerEntity.WeaponLockCounter.Retain();
		}

		private void OnBuffUpdate(IBuff buff, BuffUpdateEventType updateType) {
			
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		[field: ES3Serializable]
		public override bool IsGoodBuff { get; } = false;
		public override void OnEnds() {
			IPlayerEntity playerEntity = buffOwner as IPlayerEntity;
			if (playerEntity == null) return;
			playerEntity.WeaponLockCounter.Release();
		}

		public override void OnRecycled() {
			base.OnRecycled();
			lockedSkillEntityID.Clear();
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
		
		/*public static LockWeaponsBuff Allocate(IEntity buffDealer, IEntity buffOwner) {
			LockWeaponsBuff buff = LockWeaponsBuff.Allocate(buffDealer, buffOwner);
			return buff;
		}*/
	}
}