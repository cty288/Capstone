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

		public void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner, float maxRange,
			bool ownerTriggerHitResponse = false, bool overrideExplosionFaction = false);
	}
	
	
	[RequireComponent(typeof(HitBox))]
	public abstract class AbstractBulletViewController : PoolableGameObject, IHitResponder, IController, IBulletViewController, ICanDealDamage {
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			//owner?.OnKillDamageable(damageable);
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			//owner?.OnDealDamage(damageable, damage);
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

		public ICanDealDamage ParentDamageDealer => owner;

		//public ICanDealDamageRootEntity RootDamageDealer => owner?.RootDamageDealer;
	//	public ICanDealDamageRootViewController RootViewController => owner?.RootViewController;

		protected HashSet<GameObject> hitObjects = new HashSet<GameObject>();
		public int Damage { get; protected set; }


		[SerializeField] private float autoRecycleTime = 5f;
		[SerializeField] private bool penetrateSameFaction = false;
		[SerializeField] private bool autoDestroyWhenOwnerDestroyed = false;
		private Coroutine autoRecycleCoroutine = null;
		
		protected HitBox hitBox = null;
		protected GameObject bulletOwner = null;
		protected ICanDealDamage owner = null;
		
		protected IEntity entity = null;
		protected float maxRange;
		protected Vector3 origin;
		protected bool inited = false;
		protected bool tickType = false;
		protected TrailRenderer[] trailRenderers = null;
		protected HitData hitData;
		protected bool ownerTriggerHitResponse = true;
		[SerializeField] private bool autoRecycleWhenHit = true;
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
		protected bool overrideExplosionFaction = false;

		protected virtual void Awake() {
			hitBox = GetComponent<HitBox>();
			trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
		}
		
		
		protected void DisableAllTrailRenderers() {
			foreach (TrailRenderer trailRenderer in trailRenderers) {
				trailRenderer.Clear();
				trailRenderer.enabled = false;
			}
		}
		
		protected void EnableAllTrailRenderers() {
			foreach (TrailRenderer trailRenderer in trailRenderers) {
				trailRenderer.Clear();
				trailRenderer.enabled = true;
			}
		}

		public virtual void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner, float maxRange,
			 bool ownerTriggerHitResponse = false, bool overrideExplosionFaction = false) {
			CurrentFaction.Value = faction;
			Damage = damage;
			hitBox.StartCheckingHits(damage);
			hitBox.HitResponder = this;
			autoRecycleCoroutine = StartCoroutine(AutoRecycle());
			this.bulletOwner = bulletOwner;
			this.overrideExplosionFaction = overrideExplosionFaction;
			
			//ignore collision with bullet owner
			Collider[] bulletOwnerColliders = bulletOwner.GetComponentsInChildren<Collider>(true);
			if (bulletOwnerColliders != null) {
				//Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>());
				foreach (Collider bulletOwnerCollider in bulletOwnerColliders) {
					Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwnerCollider);
				}
			}
			this.owner = owner;
			this.maxRange = maxRange;
			origin = transform.position;
			entity = bulletOwner.GetComponent<IEntityViewController>()?.Entity;
			ICanDealDamage rootDamageDealer = (this as ICanDealDamage).GetRootDamageDealer();
			if (rootDamageDealer != null && rootDamageDealer is IEntity rootEntity) {
				rootEntity.RegisterReadyToRecycle(OnOwnerReadyToRecycle);
			}
			
			
			entity?.RetainRecycleRC();
			inited = true;
			this.ownerTriggerHitResponse = ownerTriggerHitResponse;
			EnableAllTrailRenderers();
		}

		private void OnOwnerReadyToRecycle(IEntity e) {
			if (autoDestroyWhenOwnerDestroyed) {
				RecycleToCache();
			}
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
			if (!inited) {
				return false;
			}
			hitData = data;
			if (data.Hurtbox.Owner == gameObject || data.Hurtbox.Owner == bulletOwner || hitObjects.Contains(data.Hurtbox.Owner)) {
				return false;
			}
			else { return true; }
		}

		public void HitResponse(HitData data) {
			// if (gameObject.name == "GunBullet") {
			// 	Debug.Log("HitResponse");
			// }
			if(data.Hurtbox!=null){
			  if (data.Hurtbox.Owner == bulletOwner) {
				  return;
			  }
			  hitObjects.Add(data.Hurtbox.Owner);
			}
			if(ownerTriggerHitResponse && owner is IHitResponder hitResponder){
				hitResponder.HitResponse(data);
			}
			OnHitResponse(data);
		}

		public HitData OnModifyHitData(HitData data) {
			if (owner is IHitResponder hitResponder) {
				return hitResponder.OnModifyHitData(data);
			}
			else {
				return data;
			}
		}

		protected abstract void OnHitResponse(HitData data);

		protected virtual void OnTriggerEnter(Collider other) {
			if (!inited) {
				return;
			}

			if (!other.isTrigger) {
				Rigidbody rootRigidbody = other.attachedRigidbody;
				GameObject hitObj = rootRigidbody ? rootRigidbody.gameObject : other.gameObject;
				
				if (hitObj && (bulletOwner && hitObj.transform == bulletOwner.transform) || 
				    (hitObj.transform == owner.GetRootDamageDealerTransform())) {
					return;
				}
				
				if(hitObj.TryGetComponent<IBelongToFaction>(out var belongToFaction)){
					if (belongToFaction.CurrentFaction.Value == CurrentFaction.Value && penetrateSameFaction) {
						return;
					}
				}
				
				OnHitObject(other);
				if (autoRecycleWhenHit) {
					RecycleToCache();
				}
				
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
				Collider[] bulletOwnerColliders = bulletOwner.GetComponentsInChildren<Collider>(true);
				if (bulletOwnerColliders != null) {
					//Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>());
					foreach (Collider bulletOwnerCollider in bulletOwnerColliders) {
						Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwnerCollider, false);
					}
				}
			}
			
			
			inited = false;


			hitBox.StopCheckingHits();
			//this.bulletOwner = null;
			//this.owner = null;
			entity?.ReleaseRecycleRC();
			DisableAllTrailRenderers();
			OnModifyDamageCountCallbackList.Clear();
			_onDealDamageCallback = null;
			_onKillDamageableCallback = null;
		}
		
		

		protected abstract void OnBulletRecycled();

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}