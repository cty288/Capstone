using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.Collision;
using UnityEngine;
using UnityEngine.UI;

public class MyCrossHaitVC : WeaponCrossHairViewController {
	protected override void OnAimHurtBoxStart(IHurtbox hurtbox) {
		
	}

	protected override void OnWeaponAimOutOfRange() {
		
	}

	protected override void OnWeaponStopAimOutOfRange() {
		
	}

	protected override void OnAimHurtBoxEnd(IHurtbox hurtbox) {
	
	}

	protected override void OnWeaponHit(IDamageable damageable, int damage) {
	
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

public class DefaultWeaponCrossHairViewController : WeaponCrossHairViewController {
	private Animator aimAnimator;
	
	[SerializeField] private Image[] crossHairImages;
	[SerializeField] private Image[] scopeImages;
	[SerializeField] private Color normalColor;
	[SerializeField] private Color aimWeakPointColor;
	[SerializeField] private Color shootWeakPointColor;
	[SerializeField] private float shootWeakPointAnimDuration = 0.3f;

	private bool isShootingWeakPoint = false;
	private float shootWeakPointAnimKeepTimer = 0.3f;
	private Color targetColor;
	private Transform mainBody;
	private Transform outOfRangeBody;
	private void Awake() {
		aimAnimator = transform.Find("MainBody/Aim_Default").GetComponent<Animator>();
		targetColor = normalColor;
		mainBody = transform.Find("MainBody");
		outOfRangeBody = transform.Find("OutOfRangeIndicator");
		outOfRangeBody.gameObject.SetActive(false);
	}
	

	protected override void OnAimHurtBoxStart(IHurtbox hurtbox) {
		if (hurtbox.DamageMultiplier <= 1) {
			return;
		}
		if (!isShootingWeakPoint) {
			foreach (var crossHairImage in crossHairImages) {
				crossHairImage.color = aimWeakPointColor;
			}
		}
		targetColor = aimWeakPointColor;
	}

	protected override void OnWeaponAimOutOfRange() {
		mainBody.gameObject.SetActive(false);
		outOfRangeBody.gameObject.SetActive(true);
	}

	protected override void OnWeaponStopAimOutOfRange() {
		mainBody.gameObject.SetActive(true);
		outOfRangeBody.gameObject.SetActive(false);
	}

	protected override void OnAimHurtBoxEnd(IHurtbox hurtbox) {
		if (hurtbox.DamageMultiplier <= 1) {
			return;
		}
		if (!isShootingWeakPoint) {
			foreach (var crossHairImage in crossHairImages) {
				crossHairImage.color = normalColor;
			}
		}
		targetColor = normalColor;
	}

	protected override void OnWeaponHit(IDamageable damageable, int damage) {
		aimAnimator.CrossFade("Show", 0f);
		if (currentAimHurtbox != null && currentAimHurtbox.DamageMultiplier > 1) {
			isShootingWeakPoint = true;
			shootWeakPointAnimKeepTimer = shootWeakPointAnimDuration;
			
			foreach (var crossHairImage in crossHairImages) {
				crossHairImage.color = shootWeakPointColor;
			}
		}
	}

	protected override void Update() {
		base.Update();
		if (isShootingWeakPoint) {
			shootWeakPointAnimKeepTimer -= Time.deltaTime;
			if (shootWeakPointAnimKeepTimer <= 0) {
				isShootingWeakPoint = false;
			}
		}

		if (!isShootingWeakPoint) {
			foreach (var crossHairImage in crossHairImages) {
				//lerp to target color
				crossHairImage.color = Color.Lerp(crossHairImage.color, targetColor, Time.deltaTime * 10f);
			}
		}
		
	}

	public override void OnStartHold(IResourceEntity resourceEntity) {
		
	}

	public override void OnWeaponKillTarget(IDamageable target) {
		
	}

	public override void OnWeaponScope(bool isScoped) {
		foreach (Image scopeImage in scopeImages) {
			scopeImage.gameObject.SetActive(isScoped);
		}
	}

	public override void OnWeaponShoot() {
		
	}
}
