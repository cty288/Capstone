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
	
	public interface IExplosionViewController : IController, IHitResponder {
		int Damage { get; }
		
		float Size { get; }

		public void Init(Faction faction, int damage, float size, GameObject bulletOwnerGo, ICanDealDamage owner);
	}
	
	
	[RequireComponent(typeof(ExplosionHitBox))]
	public abstract class AbstractExplosionViewController : PoolableGameObject, IHitResponder, IController, IExplosionViewController {
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
		protected List<ParticleSystem> particleSystems = new List<ParticleSystem>();
		protected IEntity entity = null;
		
		
		public void OnKillDamageable(IDamageable damageable) {
			//owner?.OnKillDamageable(damageable);
		}

		public void OnDealDamage(IDamageable damageable, int damage) {
			//owner?.OnDealDamage(damageable, damage);
		}

		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();
		public ICanDealDamage ParentDamageDealer => owner;

		/*public ICanDealDamageRootEntity RootDamageDealer => owner?.RootDamageDealer;
		public ICanDealDamageRootViewController RootViewController => owner?.RootViewController;*/

		private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
		public int Damage { get; protected set; }
		
		public float Size { get; protected set; }
		[SerializeField] private float autoRecycleTime = 2f;
		private Coroutine autoRecycleCoroutine = null;
		
		protected ExplosionHitBox hitBox = null;
		protected GameObject bulletOwner = null;
		protected ICanDealDamage owner = null;
		protected virtual void Awake() {
			hitBox = GetComponent<ExplosionHitBox>();
			particleSystems.AddRange(GetComponentsInChildren<ParticleSystem>(true));
			particleSystems.ForEach(p => p.Stop());
		}

		public virtual void Init(Faction faction, int damage, float size, GameObject bulletOwner, ICanDealDamage owner) {
			CurrentFaction.Value = faction;
			Damage = damage;
			Size = size;
			GetComponent<SphereCollider>().radius = size;
			hitBox.StartCheckingHits(damage);
			hitBox.HitResponder = this;
			autoRecycleCoroutine = StartCoroutine(AutoRecycle());
			this.bulletOwner = bulletOwner;
			this.owner = owner;

			if (bulletOwner) {
				entity = bulletOwner.GetComponent<IEntityViewController>()?.Entity;
			}
			
			entity?.RetainRecycleRC();
			particleSystems.ForEach(p => p.Play());
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
			if(owner is IHitResponder hitResponder) {
				hitResponder.HitResponse(data);
			}
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
			entity?.ReleaseRecycleRC();
			
			particleSystems.ForEach(p => p.Stop());
			OnModifyDamageCountCallbackList.Clear();
		}
		public HitData OnModifyHitData(HitData data) {
			if (owner is IHitResponder hitResponder) {
				return hitResponder.OnModifyHitData(data);
			}
			else {
				return data;
			}
		}
		protected abstract void OnBulletRecycled();

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}