using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base {
	
	public interface IBulletViewController : IController, IHitResponder {
		int Damage { get; }

		public void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner, float maxRange);
	}
	
	
	[RequireComponent(typeof(HitBox))]
	public abstract class AbstractBulletViewController : PoolableGameObject, IHitResponder, IController, IBulletViewController, ICanDealDamage {
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
		public void OnKillDamageable(IDamageable damageable) {
			owner?.OnKillDamageable(damageable);
		}

		public void OnDealDamage(IDamageable damageable, int damage) {
			owner?.OnDealDamage(damageable, damage);
		}

		private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
		public int Damage { get; protected set; }


		[SerializeField] private float autoRecycleTime = 5f;
		[SerializeField] private bool penetrateSameFaction = false;
		private Coroutine autoRecycleCoroutine = null;
		
		protected HitBox hitBox = null;
		protected GameObject bulletOwner = null;
		protected ICanDealDamage owner = null;
		protected IEntity entity = null;
		protected float maxRange;
		protected Vector3 origin;
		protected bool inited = false;
		private void Awake() {
			hitBox = GetComponent<HitBox>();
		}

		public virtual void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner, float maxRange) {
			CurrentFaction.Value = faction;
			Damage = damage;
			hitBox.StartCheckingHits(damage);
			hitBox.HitResponder = this;
			autoRecycleCoroutine = StartCoroutine(AutoRecycle());
			this.bulletOwner = bulletOwner;
			//ignore collision with bullet owner
			Collider bulletOwnerCollider = bulletOwner.GetComponent<Collider>();
			if (bulletOwnerCollider != null) {
				Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>());
			}
			this.owner = owner;
			this.maxRange = maxRange;
			origin = transform.position;
			entity = bulletOwner.GetComponent<IEntityViewController>()?.Entity;
			entity?.RetainRecycleRC();
			inited = true;
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
			if(data.Hurtbox!=null){
			  if (data.Hurtbox.Owner == bulletOwner) {
				  return;
			  }
			  hitObjects.Add(data.Hurtbox.Owner);
			}
			OnHitResponse(data);
			//RecycleToCache();
		}

		protected abstract void OnHitResponse(HitData data);

		protected virtual void OnTriggerEnter(Collider other) {
			if (!other.isTrigger) {
				if(other.transform.root.TryGetComponent<IBelongToFaction>(out var belongToFaction)){
					if (belongToFaction.CurrentFaction.Value == CurrentFaction.Value && penetrateSameFaction) {
						return;
					}
				}
				OnHitObject(other);
				RecycleToCache();
			}
		}
		
		protected abstract void OnHitObject(Collider other);


		protected virtual void Update() {
			if (!inited) {
				return;
			}
			
			if (maxRange > 0 && Vector3.Distance(transform.position, origin) > maxRange) {
				OnBulletReachesMaxRange();
				RecycleToCache();
			}
		}

		protected abstract void OnBulletReachesMaxRange();

		public override void OnRecycled() {
			base.OnRecycled();
			OnBulletRecycled();
			hitObjects.Clear();
			if (autoRecycleCoroutine != null) {
				StopCoroutine(autoRecycleCoroutine);
				autoRecycleCoroutine = null;
			}

			if (bulletOwner) {
				Collider bulletOwnerCollider = bulletOwner.GetComponent<Collider>();
				if (bulletOwnerCollider != null) {
					Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>(), false);
				}
			}
			inited = false;

			hitBox.StopCheckingHits();
			//this.bulletOwner = null;
			//this.owner = null;
			entity?.ReleaseRecycleRC();
			
		}

		protected abstract void OnBulletRecycled();

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}