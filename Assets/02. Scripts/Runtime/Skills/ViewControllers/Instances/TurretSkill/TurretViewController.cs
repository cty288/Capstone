using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Instance.TurretSkill;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.TurretSkill {
	public class TurretViewController : AbstractBasicEntityViewController<TurretEntity>, IHitResponder {
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = true;
		
		protected ICommonEntityModel commonEntityModel;
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;

		[BindCustomData("data", "install_time")]
		public float InstallTime { get; }
		
		[BindCustomData("data", "vision")]
		public float Vision { get; }
		
		[BindCustomData("data", "last_time")]
		public float LastTime { get; }
		
		[BindCustomData("data", "ammo_size")]
		public int AmmoSize { get; }
		
		[BindCustomData("data", "time_per_shot")]
		public float TimePerShot { get; }
		
		[BindCustomData("data", "damage")]
		public int Damage { get; }
		
		
		public TurretEntity Entity => BoundEntity;

		private bool isDestroyed = false;

		[SerializeField] private GameObject explosionPrefab;

		// float lastTimer = 0;
		
		protected override void Awake() {
			base.Awake();
			commonEntityModel = this.GetModel<ICommonEntityModel>();
		}

		public TurretEntity OnBuildTurretEntity(float installTime, float vision, float lastTime, int ammoSize,
			float timePerShot, int damage, int explodeDamage, float explodeRadius, int rarity) {
			if (commonEntityModel == null) {
				commonEntityModel = this.GetModel<ICommonEntityModel>();
			}
			var builder = commonEntityModel.GetBuilder<TurretEntity>(rarity);
			builder.SetProperty<float>(new PropertyNameInfo("data", "install_time"), installTime)
				.SetProperty<float>(new PropertyNameInfo("data", "vision"), vision)
				.SetProperty<float>(new PropertyNameInfo("data", "last_time"), lastTime)
				.SetProperty<int>(new PropertyNameInfo("data", "ammo_size"), ammoSize)
				.SetProperty<float>(new PropertyNameInfo("data", "time_per_shot"), timePerShot)
				.SetProperty<int>(new PropertyNameInfo("data", "damage"), damage)
				.SetProperty<int>(new PropertyNameInfo("data", "explode_damage"), explodeDamage)
				.SetProperty<float>(new PropertyNameInfo("data", "explode_radius"), explodeRadius);
			
			return builder.Build();
		}
		
		
		protected override IEntity OnBuildNewEntity() {
			
			return OnBuildTurretEntity(5, 30, 60, 50, 0.2f, 2, 50, 30, 1);
		}

		protected override void OnEntityStart() {
			Debug.Log(
				$"Turret Deployed! Install time: {BoundEntity.GetCustomDataValue<float>("data", "install_time")}\n" +
				$"Vision: {BoundEntity.GetCustomDataValue<float>("data", "vision")}\n" +
				$"Last time: {BoundEntity.GetCustomDataValue<float>("data", "last_time")}\n" +
				$"Ammo size: {BoundEntity.GetCustomDataValue<int>("data", "ammo_size")}\n" +
				$"Time per shot: {BoundEntity.GetCustomDataValue<float>("data", "time_per_shot")}\n" +
				$"Damage: {BoundEntity.GetCustomDataValue<int>("data", "damage")}");
			isDestroyed = false;

			BoundEntity.LastTime.Value = 0;
			StartCoroutine(StartSelfDestroy());
			
			BoundEntity.Ammo.RegisterOnValueChanged(OnAmmoChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		protected override void Update() {
			base.Update();
			if (BoundEntity == null || isDestroyed || BoundEntity.LastTime.Value <= 0) {
				return;
			}
			
			BoundEntity.LastTime.Value -= Time.deltaTime;
			if (BoundEntity.LastTime.Value <= 0) {
				TurretDestruct();
			}
		}

		private IEnumerator StartSelfDestroy() {
			float buildTime = BoundEntity.GetCustomDataValue<float>("data", "install_time");
			yield return new WaitForSeconds(buildTime);
			BoundEntity.LastTime.Value = BoundEntity.GetCustomDataValue<float>("data", "last_time");
		}

		private void OnAmmoChanged(int arg1, int ammo) {
			Debug.Log("[TurretViewController] Ammo changed: " + ammo);
			if (ammo <= 0) {
				TurretDestruct();
			}
		}

		private void TurretDestruct() {
			if (isDestroyed) {
				return;
			}
			StopAllCoroutines();
			
			isDestroyed = true;

			IExplosionViewController explosion =
				GameObject.Instantiate(explosionPrefab, transform.position, Quaternion.identity)
					.GetComponent<IExplosionViewController>();
			
			int damage = BoundEntity.GetCustomDataValue<int>("data", "explode_damage");
			float radius = BoundEntity.GetCustomDataValue<float>("data", "explode_radius");

			explosion.Init(BoundEntity.CurrentFaction.Value, damage, radius, gameObject, this);

			commonEntityModel.RemoveEntity(BoundEntity.UUID);
		}

		protected override void OnBindEntityProperty() {
			
		}

		public void SetParentDamageDealer(TurretSkillEntity boundEntity) {
			BoundEntity.ParentDamageDealer = boundEntity;
		}

		public BindableProperty<Faction> CurrentFaction => BoundEntity.CurrentFaction;
		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			
		}

		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; }

		Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}

		public ICanDealDamage ParentDamageDealer => BoundEntity;
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