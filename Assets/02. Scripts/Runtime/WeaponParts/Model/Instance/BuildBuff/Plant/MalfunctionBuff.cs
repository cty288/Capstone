using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.Enemies.Model;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant {
	public enum StunnRCType {
		MulfunctionBuff,
	}
	public class MalfunctionBuff: Buff<MalfunctionBuff> {
		[field: ES3Serializable] public override float MaxDuration { get; protected set; } = 5;

		[field: ES3Serializable] public override float TickInterval { get; protected set; } = -1;
		[field: ES3Serializable] public override int Priority { get; } = 1;
		
		private ICreature creature;
		public override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		public override bool IsDisplayed() {
			return true;
		}

		public override bool Validate() {
			return buffOwner is ICreature;
		}

		public override void OnInitialize() {
			creature = buffOwner as ICreature;
		}

		public override bool OnStacked(MalfunctionBuff buff) {
			return false;
		}

		public override void OnStart() {
			if (creature is IBossEntity) {
				return;
			}
			creature.StunnedCounter.Retain(StunnRCType.MulfunctionBuff);
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override bool IsGoodBuff { get; }
		public override void OnEnds() {
			if (creature is IBossEntity) {
				return;
			}

			creature.StunnedCounter.Release(StunnRCType.MulfunctionBuff);
		}
		
		public static MalfunctionBuff Allocate(IEntity buffDealer, IEntity buffOwner, float totalDuration) {
			MalfunctionBuff buff = MalfunctionBuff.Allocate(buffDealer, buffOwner);
			buff.MaxDuration = totalDuration;
			return buff;
		}
	}
}