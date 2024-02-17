using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.Player;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.StunGrenadeSkill {
	public class StunGrenadeBuff : Buff<StunGrenadeBuff>, ICanGetSystem {
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = -1;
		[field: ES3Serializable]
		public override float TickInterval { get; protected set; } = 0.5f;
		[field: ES3Serializable]
		public override int Priority { get; } = 1;
		
		private bool buffFinished = false;
		[field: ES3Serializable]
		private float malfunctionBuffTime;
		[field: ES3Serializable]
		private float powerlessBuffTime;
		[field: ES3Serializable]
		private int powerlessBuffLevel;
		
		private MalfunctionBuff addedMalfunctionBuff;
		public override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		public override bool IsDisplayed() {
			return false;
		}

		public override bool Validate() {
			return buffOwner is ICreature && buffOwner is not IPlayerEntity;
		}

		public override void OnInitialize() {
			
		}

		public override bool OnStacked(StunGrenadeBuff buff) {
			return false;
		}

		public override void OnStart() {
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
			addedMalfunctionBuff = MalfunctionBuff.Allocate(buffDealer, buffOwner, malfunctionBuffTime);
			if (!buffSystem.AddBuff(buffOwner, buffDealer, addedMalfunctionBuff)) {
				addedMalfunctionBuff.RecycleToCache();
				buffFinished = true;
				return;
			}
			
			buffOwner.RegisterOnBuffUpdate(OnBuffUpdate);
		}

		private void OnBuffUpdate(IBuff buff, BuffUpdateEventType updateType) {
			if (updateType != BuffUpdateEventType.OnEnd) {
				return;
			}
			if (buff != addedMalfunctionBuff) {
				return;
			}
			
			buffFinished = true;
			if (powerlessBuffTime <= 0) {
				return;
			}
			IBuffSystem buffSystem = this.GetSystem<IBuffSystem>();
			PowerlessBuff powerlessBuff = PowerlessBuff.Allocate(buffDealer, buffOwner, powerlessBuffLevel, powerlessBuffTime);
			if(!buffSystem.AddBuff(buffOwner, buffDealer, powerlessBuff)) {
				powerlessBuff.RecycleToCache();
			}
		}

		public override BuffStatus OnTick() {
			if (buffFinished) {
				return BuffStatus.End;
			}
			return BuffStatus.Running;
		}

		[field: ES3Serializable]
		public override bool IsGoodBuff { get; } = false;
		public override void OnEnds() {
			
		}

		public override void OnRecycled() {
			buffOwner.UnregisterOnBuffUpdate(OnBuffUpdate);
			base.OnRecycled();
			buffFinished = false;
			addedMalfunctionBuff = null;
			
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
		
		public static StunGrenadeBuff Allocate(IEntity buffDealer, IEntity buffOwner, float malfunctionBuffTime,
			float powerlessBuffTime, int powerlessBuffLevel) {
			StunGrenadeBuff buff = StunGrenadeBuff.Allocate(buffDealer, buffOwner);
			buff.malfunctionBuffTime = malfunctionBuffTime;
			buff.powerlessBuffTime = powerlessBuffTime;
			buff.powerlessBuffLevel = powerlessBuffLevel;
			return buff;
		}
	}
}