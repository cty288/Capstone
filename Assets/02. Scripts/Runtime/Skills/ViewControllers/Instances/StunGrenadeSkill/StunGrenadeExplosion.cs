using _02._Scripts.Runtime.BuffSystem;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.StunGrenadeSkill {
	public class StunGrenadeExplosion : BasicExplosion {
		
		private float malfunctionBuffTime;
		private float powerlessBuffTime;
		private int powerlessBuffLevel;
		private IBuffSystem buffSystem;

		protected override void Awake() {
			base.Awake();
			buffSystem = this.GetSystem<IBuffSystem>();
		}

		public override bool CheckHit(HitData data) {
			if (data.Hurtbox.Owner == gameObject || data.Hurtbox.Owner == bulletOwner || 
			    data.Hurtbox.Owner == owner.GetRootDamageDealerTransform()?.gameObject || hitObjects.Contains(data.Hurtbox.Owner)) {
				return false;
			}
			else { return true; }
		}

		public void SetBuffInto(float malfunctionBuffTime, float powerlessBuffTime, int powerlessBuffLevel) {
			this.malfunctionBuffTime = malfunctionBuffTime;
			this.powerlessBuffTime = powerlessBuffTime;
			this.powerlessBuffLevel = powerlessBuffLevel;
		}

		protected override void OnHitResponse(HitData data) {
			base.OnHitResponse(data);
			if(data.Hurtbox.Owner && data.Hurtbox.Owner.TryGetComponent<IEntityViewController>(out var entityViewController)) {
				if(entityViewController.Entity == null) return;
				IEntity dealer = owner.GetParentDamageDealer((p => p is IEntity)) as IEntity;
				StunGrenadeBuff stunGrenadeBuff = StunGrenadeBuff.Allocate(dealer, entityViewController.Entity,
					malfunctionBuffTime, powerlessBuffTime, powerlessBuffLevel);
				buffSystem.AddBuff(entityViewController.Entity, dealer, stunGrenadeBuff);
			}
		}
	}
}