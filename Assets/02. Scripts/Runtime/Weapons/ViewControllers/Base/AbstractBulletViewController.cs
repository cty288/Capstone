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
	
	[RequireComponent(typeof(HitBox))]
	public abstract class AbstractBulletViewController : PoolableGameObject, IHitResponder, IController {
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
		
		private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
		public int Damage { get; protected set; }
		
		[SerializeField] private float autoRecycleTime = 5f;
		private Coroutine autoRecycleCoroutine = null;
		
		protected HitBox hitBox = null;

		private void Awake() {
			hitBox = GetComponent<HitBox>();
		}

		public void Init(Faction faction, int damage) {
			CurrentFaction.Value = faction;
			Damage = damage;
			hitBox.StartCheckingHits(damage);
		}

		public override void OnStartOrAllocate() {
			base.OnStartOrAllocate();
			autoRecycleCoroutine = StartCoroutine(AutoRecycle());
		}
		
		private IEnumerator AutoRecycle() {
			yield return new WaitForSeconds(autoRecycleTime);
			if (this) {
				RecycleToCache();
			}
		}


		public bool CheckHit(HitData data) {
			
			if (data.Hurtbox.Owner == gameObject) { return false; }
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
			hitObjects.Clear();
			if (autoRecycleCoroutine != null) {
				StopCoroutine(autoRecycleCoroutine);
				autoRecycleCoroutine = null;
			}
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}