using System;
using System.Collections;
using System.Collections.Generic;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.GameResources.Model.Base;
using UnityEngine;

public class DefaultCrossHairViewController : CrossHairViewController {
	private Animator aimAnimator;

	private void Awake() {
		aimAnimator = transform.Find("Aim_Default").GetComponent<Animator>();
	}

	protected override void OnWeaponHit(IDamageable damageable, int damage) {
		aimAnimator.CrossFade("Show", 0f);
	}

	public override void OnStartHold(IResourceEntity resourceEntity) {
		
	}

	public override void OnWeaponKillTarget(IDamageable target) {
		
	}

	public override void OnWeaponScope(bool isScoped) {
		
	}

	public override void OnWeaponShoot() {
		
	}
}
