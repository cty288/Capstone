using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base {
	
	public interface IExplosionViewController : IController, IHitResponder {
		int Damage { get; }
		

		public void Init(Faction faction, int damage, GameObject bulletOwnerGo, ICanDealDamage owner);
	}
	
	
	[RequireComponent(typeof(ExplosionHitBox))]
	public abstract class AbstractExplosionViewController : PoolableGameObject, IHitResponder, IController, IExplosionViewController {
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
		public void OnKillDamageable(IDamageable damageable) {
			owner?.OnKillDamageable(damageable);
		}

		public void OnDealDamage(IDamageable damageable, int damage) {
			owner?.OnDealDamage(damageable, damage);
		}

		private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
		public int Damage { get; protected set; }
		

		[SerializeField] private float autoRecycleTime = 2f;
		private Coroutine autoRecycleCoroutine = null;
		
		protected ExplosionHitBox hitBox = null;
		protected GameObject bulletOwner = null;
		protected ICanDealDamage owner = null;
		private void Awake() {
			hitBox = GetComponent<ExplosionHitBox>();
		}

		public void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner) {
			CurrentFaction.Value = faction;
			Damage = damage;
			hitBox.StartCheckingHits(damage);
			hitBox.HitResponder = this;
			autoRecycleCoroutine = StartCoroutine(AutoRecycle());
			this.bulletOwner = bulletOwner;
			this.owner = owner;
		}

		public override void OnStartOrAllocate() {
			base.OnStartOrAllocate();
			
		}
		
		private IEnumerator AutoRecycle() {
			yield return new WaitForSeconds(autoRecycleTime);
			if (this) {
				RecycleToCache();
			}
		}


		public bool CheckHit(HitData data) {

			if (data.Hurtbox.Owner == gameObject || data.Hurtbox.Owner == bulletOwner || hitObjects.Contains(data.Hurtbox.Owner)) {
				return false;
			}
			else { return true; }
		}

		public void HitResponse(HitData data) {
			hitObjects.Add(data.Hurtbox.Owner);
			OnHitResponse(data);
		}

		protected abstract void OnHitResponse(HitData data);

		public override void OnRecycled() {
			base.OnRecycled();
			OnBulletRecycled();
			hitObjects.Clear();
			if (autoRecycleCoroutine != null) {
				StopCoroutine(autoRecycleCoroutine);
				autoRecycleCoroutine = null;
			}
			hitBox.StopCheckingHits();
			this.bulletOwner = null;
			this.owner = null;
			
		}

		protected abstract void OnBulletRecycled();

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}