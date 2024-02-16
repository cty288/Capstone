using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Currency.Model;
using BehaviorDesigner.Runtime.Tasks;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
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
	
	public abstract class WeaponPartsBuff<TWeaponParts, TBuffType> : PropertyBuff<TBuffType>, ICanRegisterEvent, IWeaponPartsBuff<TWeaponParts> 
		where TWeaponParts : class, IWeaponPartsEntity
	 where TBuffType : WeaponPartsBuff<TWeaponParts, TBuffType>, new() {
		
		public override bool IsGoodBuff => true;

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


		public override bool OnStacked(TBuffType buff) {
			return true;
		}

		private bool descriptionAdded = false;

		public override void OnInitialize(IEntity buffDealer, IEntity entity, bool force = false) {
			weaponPartsEntity = buffDealer as TWeaponParts;
			weaponEntity = entity as IWeaponEntity;


			if (force || this.BuffOwnerID == null || this.BuffOwnerID != entity?.UUID ||
			    this.BuffDealerID != buffDealer?.UUID) {
				AddDescription();
			}
			base.OnInitialize(buffDealer, entity, force);
		}

		protected void AddDescription() {
			if (descriptionAdded) {
				return;
			}
			descriptionAdded = true;
			CurrencyType currencyType = weaponPartsEntity.GetBuildType();
			WeaponPartType partType = weaponPartsEntity.WeaponPartType;
			additionalResourcePropertyDescriptionGetters = OnRegisterResourcePropertyDescriptionGetters(
				$"PropertyIcon{currencyType.ToString()}",
				null);
			if (additionalResourcePropertyDescriptionGetters != null) {
				weaponEntity?.AddAdditionalResourcePropertyDescriptionGetters(
					additionalResourcePropertyDescriptionGetters);
			}
		}

		
		public override void OnAwake() {
			base.OnAwake();
		}

		public override void OnStart() {
			AddDescription();
		}

		protected Dictionary<Type, TypeEventSystem.IRegisterations> eventRegisterations =
			new Dictionary<Type, TypeEventSystem.IRegisterations>();

		protected void RegisterWeaponBuildBuffEvent<TEvent>(Action<TEvent> callback) where TEvent : WeaponBuildBuffEvent {
			Type type = typeof(TEvent);
			TypeEventSystem.IRegisterations registerations = null;
			
			if (eventRegisterations.TryGetValue(type, out registerations)) {

			}
			else {
				registerations = new TypeEventSystem.Registerations<TEvent>();
				eventRegisterations.Add(type,registerations);
			}

			(registerations as TypeEventSystem.Registerations<TEvent>).OnEvent += callback;
			
			this.RegisterEvent<TEvent>(DoOnWeaponBuildBuffEvent);
			
		}

		private void DoOnWeaponBuildBuffEvent<TEvent>(TEvent e) where TEvent : WeaponBuildBuffEvent {
			if(e.WeaponEntity == null || e.WeaponEntity != weaponEntity) {
				return;
			}
			
			TypeEventSystem.IRegisterations registerations = null;
			if (eventRegisterations.TryGetValue(typeof(TEvent), out registerations)) {
				(registerations as TypeEventSystem.Registerations<TEvent>).OnEvent.Invoke(e);
			}
		}
		
		protected void UnRegisterWeaponBuildBuffEvent<TEvent>(Action<TEvent> callback) where TEvent : WeaponBuildBuffEvent {
			Type type = typeof(TEvent);
			TypeEventSystem.IRegisterations registerations = null;
			
			if (eventRegisterations.TryGetValue(type, out registerations)) {
				(registerations as TypeEventSystem.Registerations<TEvent>).OnEvent -= callback;
			}

			this.UnRegisterEvent<TEvent>(DoOnWeaponBuildBuffEvent);
		}

		public abstract List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title);

		
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
			descriptionAdded = false;
		}

		public IWeaponPartsEntity WeaponPartsEntity => weaponPartsEntity;

		public override void OnRecycled() {
			base.OnRecycled();
			additionalResourcePropertyDescriptionGetters = null;
			eventRegisterations.Clear();
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}