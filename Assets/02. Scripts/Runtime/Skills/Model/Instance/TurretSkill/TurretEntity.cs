using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Temporary;
using Runtime.Utilities.Collision;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill {
	public class TurretEntity : AbstractBasicEntity, IHitResponder {
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
		
		public BindableProperty<int> Ammo = new BindableProperty<int>(0);
		public BindableProperty<float> LastTime = new BindableProperty<float>(0);

		[field: ES3Serializable]
		public override string EntityName { get; set; } = "TurretEntity";
		protected override ConfigTable GetConfigTable() {
			return null;
		}

		protected override void OnEntityStart(bool isLoadedFromSave) {
			Ammo.Value = GetCustomDataValue<int>("data", "ammo_size");
			LastTime.Value = 0;
		}
		
		public void UseAmmo() {
			Ammo.Value--;
		}

		public override void OnDoRecycle() {
			SafeObjectPool<TurretEntity>.Singleton.Recycle(this);
		}

		public override void OnRecycle() {
			
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return null;
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override void OnEntityRegisterAdditionalProperties() {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return new[] {
				new CustomProperty("data",
					new CustomDataProperty<float>("install_time"),
					new CustomDataProperty<float>("vision"),
					new CustomDataProperty<float>("last_time"),
					new CustomDataProperty<int>("ammo_size"),
					new CustomDataProperty<float>("time_per_shot"),
					new CustomDataProperty<int>("damage"),
					new CustomDataProperty<int>("explode_damage"),
					 new CustomDataProperty<float>("explode_radius"))
			};

		}

		[field: ES3Serializable]
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
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

		public ICanDealDamage ParentDamageDealer { get; set; }
		public bool CheckHit(HitData data) {
			return true;
		}

		public void HitResponse(HitData data) {
			
		}

		public HitData OnModifyHitData(HitData data) {
			return data;
		}
	}
}