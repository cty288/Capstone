using _02._Scripts.Runtime.BuffSystem.ConfigurableBuff;
using Runtime.DataFramework.Entities;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.WeaponParts.Model.Base {
	public abstract class WeaponBuildBuff<T> : ConfigurableBuff<T> where T : WeaponBuildBuff<T>, new(){
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = -1;
		public override int Priority { get; } = 1;
		
		protected IWeaponEntity weaponEntity;
		
		public override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		public override bool IsDisplayed() {
			return false;
		}

		public override bool Validate() {
			return base.Validate() && buffOwner is IWeaponEntity && OnValidate();
		}

		protected abstract bool OnValidate();

		public override bool IsGoodBuff => true;

		protected override void OnLevelUp() {
			
		}

		public override void OnInitialize(IEntity buffDealer, IEntity entity, bool force = false) {
			weaponEntity = entity as IWeaponEntity;
			base.OnInitialize(buffDealer, entity, force);
		}

		protected override void OnBuffStacked(T buff) {
			
		}
	}
}