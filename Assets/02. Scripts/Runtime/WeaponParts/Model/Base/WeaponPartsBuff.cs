using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.WeaponParts.Model.Base {
	public interface IWeaponPartsBuff : IBuff {
		IWeaponPartsEntity WeaponPartsEntity { get; }
	}
	public interface IWeaponPartsBuff<TWeaponParts> : IBuff, IWeaponPartsBuff where TWeaponParts : class, IWeaponPartsEntity {
		
	}
	
	public abstract class WeaponPartsBuff<TWeaponParts, TBuffType> : PropertyBuff<TBuffType>, IWeaponPartsBuff<TWeaponParts> 
		where TWeaponParts : class, IWeaponPartsEntity
	 where TBuffType : WeaponPartsBuff<TWeaponParts, TBuffType>, new(){
		
		[field: ES3Serializable]
		public override float MaxDuration { get; protected set; } = -1;
		public override int Priority { get; } = 1;
		protected TWeaponParts weaponPartsEntity;
		protected IWeaponEntity weaponEntity;
		protected List<GetResourcePropertyDescriptionGetter> additionalResourcePropertyDescriptionGetters;
		public override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		public override bool IsDisplayed() {
			return false;
		}


		public override void OnStacked(TBuffType buff) {
			
		}

		public override void OnInitialize(IEntity buffDealer, IEntity entity, bool force = false) {
			weaponPartsEntity = buffDealer as TWeaponParts;
			weaponEntity = entity as IWeaponEntity;
			base.OnInitialize(buffDealer, entity, force);
		}

		public override void OnAwake() {
			base.OnAwake();
			additionalResourcePropertyDescriptionGetters = OnRegisterResourcePropertyDescriptionGetters();
			if (additionalResourcePropertyDescriptionGetters != null) {
				weaponEntity?.AddAdditionalResourcePropertyDescriptionGetters(additionalResourcePropertyDescriptionGetters);
			}
		}

		public abstract List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters();

		
		public override bool Validate() {
			return base.Validate() && buffOwner is IWeaponEntity;

		}

		public static TBuffType CreateBuff(IEntity weaponParts, IEntity weaponEntity) {
			TBuffType buff = Allocate(weaponParts, weaponEntity);
			return buff;
		}

		public override void OnEnds() {
			base.OnEnds();
			if (additionalResourcePropertyDescriptionGetters != null) {
				weaponEntity.RemoveAdditionalResourcePropertyDescriptionGetters(additionalResourcePropertyDescriptionGetters);
			}
			
		}

		public IWeaponPartsEntity WeaponPartsEntity => weaponPartsEntity;

		public override void OnRecycled() {
			base.OnRecycled();
			additionalResourcePropertyDescriptionGetters = null;
		}
	}
}