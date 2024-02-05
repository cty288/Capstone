using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities.Collision;
using UnityEngine;

public class MeleeBladeViewController : AbstractMikroController<MainGame>, IHitResponder, IController, ICanDealDamage {
	[SerializeField] private float meleeForce = 40f;
	public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Hostile);
	
	/*public ICanDealDamageRootEntity RootDamageDealer => owner?.RootDamageDealer;
	public ICanDealDamageRootViewController RootViewController => owner?.RootViewController;*/
	
	public int Damage { get; protected set; }
	
	protected HitBox hitBox = null;
	protected GameObject bulletOwner = null;
	protected ICanDealDamage owner = null;
	protected IEntity entity = null;


	protected bool inited = false;
	protected HitData hitData;
	protected Collider collider;
	protected virtual void Awake() {
		hitBox = GetComponent<HitBox>();
		collider = GetComponent<Collider>();
	}

	public virtual void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner) {
		CurrentFaction.Value = faction;
		Damage = damage;
		//hitBox.StartCheckingHits(damage);
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
		inited = true;
	}

	public void StartCheckHit() {
		collider.enabled = true;
		hitBox.StartCheckingHits(Damage);
	}
	
	public void StopCheckHit() {
		collider.enabled = false;
		hitBox.StopCheckingHits();
	}
	
	public void OnKillDamageable(IDamageable damageable) {
		//owner?.OnKillDamageable(damageable);
	}

	public void OnDealDamage(IDamageable damageable, int damage) {
		//owner?.OnDealDamage(damageable, damage);
		
	}

	public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();
	public ICanDealDamage ParentDamageDealer => owner;

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
		if(data.Hurtbox!=null){
			if (data.Hurtbox.Owner == bulletOwner) {
				return;
			}
			Rigidbody rb = data.Hurtbox.Owner.GetComponent<Rigidbody>();
			if (rb) {
				Vector3 dir = rb.position - bulletOwner.transform.position;
				dir.y = 0;
				dir = Quaternion.AngleAxis(45, Vector3.Cross(dir, Vector3.up)) * dir;
				dir.Normalize();
				rb.AddForce(dir * meleeForce, ForceMode.Impulse);
			}
		}
	}

	public HitData OnModifyHitData(HitData data) {
		return data;
	}
}
