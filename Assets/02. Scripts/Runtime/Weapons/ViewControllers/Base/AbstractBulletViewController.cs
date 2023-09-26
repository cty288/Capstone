using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Weapons.ViewControllers.Base {
	
	public interface IBulletViewController : IController, IHitResponder {
		int Damage { get; }

		public void Init(Faction faction, int damage, GameObject bulletOwner);
	}
	
	
	[RequireComponent(typeof(HitBox))]
	public abstract class AbstractBulletViewController : PoolableGameObject, IHitResponder, IController, IBulletViewController {
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
		
		private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
		public int Damage { get; protected set; }
		
		[SerializeField] private float autoRecycleTime = 5f;
		private Coroutine autoRecycleCoroutine = null;
		
		protected HitBox hitBox = null;
		protected GameObject bulletOwner = null;

		private void Awake() {
			hitBox = GetComponent<HitBox>();
		}

		public void Init(Faction faction, int damage, GameObject bulletOwner) {
			CurrentFaction.Value = faction;
			Damage = damage;
			hitBox.StartCheckingHits(damage);
			hitBox.HitResponder = this;
			autoRecycleCoroutine = StartCoroutine(AutoRecycle());
			this.bulletOwner = bulletOwner;
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
			RecycleToCache();
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
			
		}

		protected abstract void OnBulletRecycled();

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}