using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

public class SanctuaryShieldViewController : AbstractMikroController<MainGame>, IHurtResponder, IController {
	public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
	
	protected HurtBox hurtbox = null;
	protected IWeaponViewController weaponOwner = null;

	protected bool inited = false;
	protected HitData hitData;
	protected Collider collider;
	
	protected int maxHealth;
	protected int currentHealth;
	public Material shieldMaterial;
	private Color originalColor;
	
	protected virtual void Awake() {
		hurtbox = GetComponentInChildren<HurtBox>();
		collider = GetComponentInChildren<Collider>();
		if(shieldMaterial == null)
			shieldMaterial = gameObject.GetComponent<Renderer>().material;
	}

	public void Init(IWeaponViewController weaponOwner, GameObject characterOwner)
	{
		this.weaponOwner = weaponOwner;
		hurtbox.SetOwner(characterOwner);
		hurtbox.HurtResponder = this;
		
		maxHealth = this.weaponOwner.WeaponEntity.GetCustomDataValue<int>("shield", "health").Value;
		currentHealth = maxHealth;
		originalColor = shieldMaterial.GetColor("_DamageColor");
		
		shieldMaterial.SetFloat("_Health", 1);
		shieldMaterial.SetFloat("_Manifest", 0);
		shieldMaterial.DOFloat(2, "_Manifest", 0.75f);
	}

	public bool CheckHurt(HitData data)
	{
		return data.Attacker.CurrentFaction.Value != CurrentFaction.Value;
	}	

	public void HurtResponse(HitData data)
	{
		currentHealth -= data.Damage;
		//update shield shader health
		float percentage = (float)currentHealth / maxHealth;
		shieldMaterial.DOFloat(percentage, "_Health", 0.15f);
		if(currentHealth <= 0)
		{
			FlashAndBreak();
		}
	}

	private async UniTask FlashAndBreak()
	{
		//flash
		Color flashColor = new Color(1,1,1,0.5f);
		shieldMaterial.DOColor(flashColor, "_DamageColor", 0.2f).SetLoops(3);
		
		await UniTask.WaitForSeconds(0.65f);
		
		shieldMaterial.DOColor(Color.white, "_DamageColor", 0.05f);
		
		await UniTask.WaitForSeconds(0.1f);
		
		shieldMaterial.SetColor("_DamageColor", originalColor);
		gameObject.SetActive(false);
	}
}
