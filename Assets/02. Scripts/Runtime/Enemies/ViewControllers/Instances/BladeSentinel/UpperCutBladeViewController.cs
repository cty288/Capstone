using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _02._Scripts.Runtime.Utilities.AsyncTriggerExtension;
using Cysharp.Threading.Tasks;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

public class UpperCutBladeViewController : PoolableGameObject, IHitResponder, IController, ICanDealDamage {

	private Animator animator;
	
	[SerializeField] private bool autoDestroyWhenOwnerDestroyed = true;
	public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Hostile);
	public void OnKillDamageable(IDamageable damageable) {
		//owner?.OnKillDamageable(damageable);
	}

	public void OnDealDamage(IDamageable damageable, int damage) {
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
	
	public int Damage { get; protected set; }


	
	
	protected HitBox hitBox = null;
	protected GameObject bulletOwner = null;
	protected ICanDealDamage owner = null;
	protected IEntity entity = null;


	protected bool inited = false;
	protected HitData hitData;
	private Action<IDamageable, int> _onDealDamageCallback;
	private Action<IDamageable> _onKillDamageableCallback;


	protected virtual void Awake() {
		hitBox = GetComponentInChildren<HitBox>();
		animator = GetComponentInChildren<Animator>();
	}
	

	public virtual void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner) {
		CurrentFaction.Value = faction;
		Damage = damage;
		hitBox.StartCheckingHits(damage);
		hitBox.HitResponder = this;
		this.bulletOwner = bulletOwner;
		
		//ignore collision with bullet owner
		Collider[] bulletOwnerColliders = bulletOwner.GetComponentsInChildren<Collider>(true);
		if (bulletOwnerColliders != null) {
			//Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>());
			Collider selfCollider = GetComponentInChildren<Collider>(true);
			foreach (Collider bulletOwnerCollider in bulletOwnerColliders) {
				Physics.IgnoreCollision(selfCollider, bulletOwnerCollider);
			}
		}
		this.owner = owner;
		entity = bulletOwner.GetComponent<IEntityViewController>()?.Entity;

		ICanDealDamage root = (this as ICanDealDamage).GetRootDamageDealer();
		if (root != null && root is IEntity rootEntity) {
			rootEntity.RegisterReadyToRecycle(OnOwnerReadyToRecycle);
		}
		
		entity?.RetainRecycleRC();
		inited = true;

		BladeStay(2f);
	}

	private async UniTask BladeStay(float time) {
		await UniTask.WaitForSeconds(time, false, PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		animator.SetTrigger("Finish");
		await UniTask.WaitForSeconds(0.3f, false, PlayerLoopTiming.Update, gameObject.GetCancellationTokenOnDestroyOrRecycleOrDie());
		RecycleToCache();
	}
	

	private void OnOwnerReadyToRecycle(IEntity e) {
		if (autoDestroyWhenOwnerDestroyed) {
			RecycleToCache();
		}
	}

	public override void OnStartOrAllocate() {
		base.OnStartOrAllocate();
		
	}
	

	public bool CheckHit(HitData data) {
		if (!inited) {
			return false;
		}
		hitData = data;
		if (data.Hurtbox.Owner == gameObject || data.Hurtbox.Owner == bulletOwner) {
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
		}
	}

	public HitData OnModifyHitData(HitData data) {
		return data;
	}


	public override void OnRecycled() {
		base.OnRecycled();
		if (bulletOwner) {
			Collider[] bulletOwnerColliders = bulletOwner.GetComponentsInChildren<Collider>(true);
			if (bulletOwnerColliders != null) {
				//Physics.IgnoreCollision(GetComponent<Collider>(), bulletOwner.GetComponent<Collider>());
				foreach (Collider bulletOwnerCollider in bulletOwnerColliders) {
					Collider selfCollider = GetComponentInChildren<Collider>(true);
					Physics.IgnoreCollision(selfCollider, bulletOwnerCollider, false);
				}
			}
		}
		
		
		inited = false;
		hitBox.StopCheckingHits();
		entity?.ReleaseRecycleRC();
		OnModifyDamageCountCallbackList.Clear();
		_onDealDamageCallback = null;
		_onKillDamageableCallback = null;
	}


	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
}
