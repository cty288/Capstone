using System.Collections;
using System.Collections.Generic;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

public class BerserkerSlashBullet : AbstractBulletViewController {
	protected float bulletSpeed;
	private Vector3 direction;
	private ParticleSystem[] particles;
	protected override void Awake() {
		base.Awake();
		particles = GetComponentsInChildren<ParticleSystem>(true);
	}

	protected override void Update() {
		base.Update();
		transform.position += direction * bulletSpeed * Time.deltaTime;
	}

	protected override void OnHitResponse(HitData data) {
		
	}

	public override void Init(Faction faction, int damage, GameObject bulletOwner, ICanDealDamage owner, float maxRange,
		bool ownerTriggerHitResponse = false, bool overrideExplosionFaction = false) {
		base.Init(faction, damage, bulletOwner, owner, maxRange, ownerTriggerHitResponse, overrideExplosionFaction);
		foreach (ParticleSystem particle in particles) {
			ParticleSystem.MainModule main = particle.main;
			main.loop = true;
			particle.Play();
		}
	}

	public void SetData(float bulletSpeed, Vector3 direction) {
		this.bulletSpeed = bulletSpeed;
		this.direction = direction.normalized;
		transform.rotation = Quaternion.LookRotation(direction);
		
	}
	protected override void OnHitObject(Collider other) {
		foreach (ParticleSystem particle in particles) {
			ParticleSystem.MainModule main = particle.main;
			main.loop = false;
		}
		StopCoroutine(autoRecycleCoroutine);
		autoRecycleCoroutine = StartCoroutine(AutoRecycle());
	}

	private IEnumerator AutoRecycle() {
		yield return new WaitForSeconds(5f);
		RecycleToCache();
	}

	protected override void OnBulletReachesMaxRange() {
		
	}

	protected override void OnBulletRecycled() {
		
	}
}
