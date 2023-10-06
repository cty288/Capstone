using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.GameResources.Model.Base;
using Runtime.Weapons.Model.Base;
using UnityEngine;

public interface ICrossHairViewController {
    void OnStart(IResourceEntity resourceEntity);
    
    void OnStopHold();
    
    void OnKill(IDamageable target);
    
    void OnScope(bool isScoped);

    void OnShoot();
}
public abstract class CrossHairViewController : DefaultPoolableGameObject, ICrossHairViewController, IController {
    public void OnStart(IResourceEntity resourceEntity) {
        OnStartHold(resourceEntity);
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
