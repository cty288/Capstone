using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public interface ICrossHairViewController {
    void OnStart(IResourceEntity resourceEntity, GameObject self);
    
    void OnStopHold();
    
    void OnKill(IDamageable target);
    
    void OnScope(bool isScoped);

    void OnShoot();
    void OnHit(IDamageable damageable, int damage);
    
    void OnAimHurtBoxEnter(IHurtbox hurtbox);
    
    void OnAimHurtBoxExit(IHurtbox hurtbox);
}
public abstract class WeaponCrossHairViewController : DefaultPoolableGameObject, ICrossHairViewController, IController {
    protected IHurtbox currentAimHurtbox = null;
    private IHurtbox lastAimHurtbox = null;
    protected IWeaponEntity weaponEntity = null;
    protected float weaponRange;
    protected GameObject self;
    private bool isInRange = false;
    public void OnStart(IResourceEntity resourceEntity, GameObject self) {
        weaponEntity = resourceEntity as IWeaponEntity;
        this.self = self;
        weaponEntity?.GetRange().RealValue.RegisterWithInitValue(OnRangeChanged)
            .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        OnStartHold(resourceEntity);
    }

    private void OnRangeChanged(float arg1, float range) {
        this.weaponRange = range;
    }
    
    protected bool IsInRange(Vector3 targetPosition) {
        return Vector3.Distance(self.transform.position, targetPosition) <= weaponRange;
    }

    public void OnStopHold() {
        //RecycleToCache();
    }

    public void OnKill(IDamageable target) {
        OnWeaponKillTarget(target);
    }

    public void OnScope(bool isScoped) {
        OnWeaponScope(isScoped);
    }

    public void OnShoot() {
        OnWeaponShoot();
    }

    protected virtual void Update() {
        if (currentAimHurtbox == null || !currentAimHurtbox.Owner) {
            isInRange = false;
        }
        else {
            bool lastInRange = isInRange;
            isInRange = IsInRange(currentAimHurtbox.Owner.transform.position);
            if (lastInRange != isInRange || lastAimHurtbox != currentAimHurtbox) {
                if (isInRange) {
                    OnAimHurtBoxStart(currentAimHurtbox);
                    OnWeaponStopAimOutOfRange();
                }
                else {
                    OnAimHurtBoxEnd(currentAimHurtbox);
                    OnWeaponAimOutOfRange();
                }
            }
        }
        lastAimHurtbox = currentAimHurtbox;
    }

    public void OnHit(IDamageable damageable, int damage) {
        OnWeaponHit(damageable, damage);
    }

    public void OnAimHurtBoxEnter(IHurtbox hurtbox) {
        currentAimHurtbox = hurtbox;
        //OnAimHurtBoxStart(hurtbox);
    }

    protected abstract void OnAimHurtBoxStart(IHurtbox hurtbox);
    
    protected abstract void OnWeaponAimOutOfRange();
    
    protected abstract void OnWeaponStopAimOutOfRange();

    public void OnAimHurtBoxExit(IHurtbox hurtbox) {
        OnAimHurtBoxEnd(hurtbox);
        OnWeaponStopAimOutOfRange();
        currentAimHurtbox = null;
    }

    protected abstract void OnAimHurtBoxEnd(IHurtbox hurtbox);

    /// <summary>
    /// Only for weapons
    /// </summary>
    /// <param name="target"></param>
    protected abstract void OnWeaponHit(IDamageable damageable, int damage);


    public abstract void OnStartHold(IResourceEntity resourceEntity);
    
    /// <summary>
    /// Only for weapons
    /// </summary>
    /// <param name="target"></param>
    public abstract void OnWeaponKillTarget(IDamageable target);
    
    /// <summary>
    /// Only for weapons
    /// </summary>
    /// <param name="target"></param>
    public abstract void OnWeaponScope(bool isScoped);
        
    /// <summary>
    /// Only for weapons
    /// </summary>
    /// <param name="target"></param>
    public abstract void OnWeaponShoot();

    
    
    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
