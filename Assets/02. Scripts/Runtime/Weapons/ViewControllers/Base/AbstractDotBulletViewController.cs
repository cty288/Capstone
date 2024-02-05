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

namespace Runtime.Weapons.ViewControllers.Base
{




	[RequireComponent(typeof(DotHitBox))]
	public abstract class AbstractDotBulletViewController : PoolableGameObject, IHitResponder, IController, IBulletViewController, ICanDealDamage
	{
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
		public void OnKillDamageable(IDamageable damageable)
		{
			//owner?.OnKillDamageable(damageable);
		}

		public void OnDealDamage(IDamageable damageable, int damage)
		{
			//owner?.OnDealDamage(damageable, damage);

		}

		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

		Action<IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		Action<IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}

		public ICanDealDamage ParentDamageDealer => owner;

		/*public ICanDealDamageRootEntity RootDamageDealer => owner?.RootDamageDealer;
		public ICanDealDamageRootViewController RootViewController => owner?.RootViewController;*/

		protected HashSet<GameObject> hitObjects = new HashSet<GameObject>();
		public int Damage { get; protected set; }


		[SerializeField] private float autoRecycleTime = 5f;
		[SerializeField] private bool penetrateSameFaction = false;
		[SerializeField] private bool autoDestroyWhenOwnerDestroyed = false;
		private Coroutine autoRecycleCoroutine = null;

		protected DotHitBox hitBox = null;
		protected GameObject bulletOwner = null;
		protected ICanDealDamage owner = null;
		protected IEntity entity = null;
		protected float maxRange;
		protected Vector3 origin;
		protected bool inited = false;
		protected bool tickType = false;
		protected TrailRenderer[] trailRenderers = null;
		protected HitData hitData;
		private Action<IDamageable, int> _onDealDamageCallback;
		private Action<IDamageable> _onKillDamageableCallback;

		private void Awake()
		{
			hitBox = GetComponent<DotHitBox>();
			trailRenderers = GetComponentsInChildren<TrailRenderer>(true);
		}


		protected void DisableAllTrailRenderers()
		{
			foreach (TrailRenderer trailRenderer in trailRenderers)
			{
				trailRenderer.Clear();
				trailRenderer.enabled = false;
			}
		}

		protected void EnableAllTrailRenderers()
		{
			foreach (TrailRenderer trailRenderer in trailRenderers)
			{
				trailRenderer.Clear();
				trailRenderer.enabled = true;
			}
		}
		public HitData OnModifyHitData(HitData data) {
			if (owner is IHitResponder hitResponder) {
				return hitResponder.OnModifyHitData(data);
			}
			else {
				return data;
			}
		}
		public virtual void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner, float maxRange,
			bool ownerTriggerHitResponse = false)
		{
			CurrentFaction.Value = faction;
			Damage = damage;
			hitBox.StartCheckingHits(damage);
			hitBox.HitResponder = this;
			autoRecycleCoroutine = StartCoroutine(AutoRecycle());
			this.bulletOwner = bulletOwner;

			//ignore collision with bullet owner
			Collider bulletOwnerCollider = bulletOwner.GetComponent<Collider>();
			if (bulletOwnerCollider != null)
			{
				Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>());
			}
			this.owner = owner;
			this.maxRange = maxRange;
			origin = transform.position;
			entity = bulletOwner.GetComponent<IEntityViewController>()?.Entity;

			ICanDealDamage ownerDamageDealer = owner?.ParentDamageDealer;
			if(ownerDamageDealer != null && ownerDamageDealer is IEntity rootEntity) {
				rootEntity.RegisterReadyToRecycle(OnOwnerReadyToRecycle);
			}
			
			entity?.RetainRecycleRC();
			inited = true;
			EnableAllTrailRenderers();
		}

		private void OnOwnerReadyToRecycle(IEntity e)
		{
			if (autoDestroyWhenOwnerDestroyed)
			{
				RecycleToCache();
			}
		}

		public override void OnStartOrAllocate()
		{
			base.OnStartOrAllocate();

		}

		private IEnumerator AutoRecycle()
		{
			yield return new WaitForSeconds(autoRecycleTime);
			if (this)
			{
				RecycleToCache();
			}
		}


		public bool CheckHit(HitData data)
		{
			hitData = data;
			if (data.Hurtbox.Owner == gameObject || data.Hurtbox.Owner == bulletOwner)
			{
				return false;
			}
			else { return true; }
		}

		public void HitResponse(HitData data)
		{
			// if (gameObject.name == "GunBullet") {
			// 	Debug.Log("HitResponse");
			// }
			if (data.Hurtbox != null)
			{
				if (data.Hurtbox.Owner == bulletOwner)
				{
					return;
				}
				//hitObjects.Add(data.Hurtbox.Owner);
			}
			OnHitResponse(data);
			//RecycleToCache();
		}

		protected abstract void OnHitResponse(HitData data);

		protected virtual void OnTriggerEnter(Collider other)
		{
			// if (gameObject.name == "GunBullet") {
			// 	Debug.Log("HitResponse");
			// }
			if (!other.isTrigger)
			{
				Rigidbody rootRigidbody = other.attachedRigidbody;
				GameObject hitObj =
					rootRigidbody ? rootRigidbody.gameObject : other.gameObject;

				if (hitObj != null && hitObj.transform == bulletOwner.transform)
				{
					return;
				}
				if (hitObj.TryGetComponent<IBelongToFaction>(out var belongToFaction))
				{
					if (belongToFaction.CurrentFaction.Value == CurrentFaction.Value && penetrateSameFaction)
					{
						return;
					}
				}

				OnHitObject(other);
				RecycleToCache();
			}
		}


		protected abstract void OnHitObject(Collider other);


		protected virtual void Update()
		{
			if (!inited)
			{
				return;
			}

			if (maxRange > 0 && Vector3.Distance(transform.position, origin) > maxRange)
			{
				OnBulletReachesMaxRange();
				RecycleToCache();
			}
		}

		protected abstract void OnBulletReachesMaxRange();

		public override void OnRecycled()
		{
			base.OnRecycled();
			OnBulletRecycled();
			hitObjects.Clear();
			if (autoRecycleCoroutine != null)
			{
				StopCoroutine(autoRecycleCoroutine);
				autoRecycleCoroutine = null;
			}

			if (bulletOwner)
			{
				Collider bulletOwnerCollider = bulletOwner.GetComponent<Collider>();
				if (bulletOwnerCollider != null)
				{
					Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>(), false);
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

		public IArchitecture GetArchitecture()
		{
			return MainGame.Interface;
		}
	}
}