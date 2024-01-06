using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using MikroFramework.Pool;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using UnityEngine;

public enum BuffStatus {
    Running,
    End
}

public struct BuffDisplayInfo {
    public bool Display;
    public string IconPrefab;
    public string BuffDisplayName;
    public string BuffDescription;

    public BuffDisplayInfo(bool display, string iconPrefabName, string buffDisplayName, string buffDescription) {
        Display = display;
        IconPrefab = iconPrefabName;
        BuffDisplayName = buffDisplayName;
        BuffDescription = buffDescription;
    }

    public BuffDisplayInfo(bool display = false) {
        Display = false;
        IconPrefab = null;
        BuffDisplayName = null;
        BuffDescription = null;
    
    }
}


public interface IBuff {
    public float MaxDuration { get; }
    public float RemainingDuration { get; set; }
    public float TickInterval { get; }
    public float TickTimer { get; set; }
    
    public int Priority { get; }
    public string BuffDealerID { get; }
    
    public string BuffOwnerID { get; }
    
    public BuffDisplayInfo OnGetBuffDisplayInfo();
    
    bool Validate();
    
    void OnInitialize(IEntity buffDealer, IEntity entity);
    void OnStacked(IBuff buff);
    
    void OnAwake();
    
    void OnStart();
    BuffStatus OnTick();
    void OnEnd();
}


public abstract class Buff<T> : IBuff, IPoolable where T : Buff<T>, new() {
    
    public abstract float MaxDuration { get; protected set;}
    
    [field: ES3Serializable]
    public float RemainingDuration { get; set; }
    
    public abstract float TickInterval { get; protected set; }
    
    [field: ES3Serializable]
    public float TickTimer { get; set; }

    public abstract int Priority { get; }
    
    [field: ES3Serializable]
    public string BuffDealerID { get; protected set; }
    
    [field: ES3Serializable]
    public string BuffOwnerID  { get; protected set; }

    public BuffDisplayInfo OnGetBuffDisplayInfo() {
        if (!IsDisplayed()) {
            return new BuffDisplayInfo();
        }
        else {
            string typeName = this.GetType().Name;
            return new BuffDisplayInfo(true, $"{typeName}_Icon",
                Localization.Get($"{typeName}_Name"),
                OnGetDescription($"{typeName}_Desc"));
        }
    }
    
    public abstract string OnGetDescription(string defaultLocalizationKey);
    
    

    public abstract bool IsDisplayed();

    public abstract bool Validate();
    
    protected IEntity buffDealer;
    protected IEntity buffOwner;
    
    
    /// <summary>
    /// executed only once when the buff is created, or when the buff is loaded from save. This runs before Validate
    /// </summary>
    /// <param name="buffDealer"></param>
    /// <param name="entity"></param>
    public virtual void OnInitialize(IEntity buffDealer, IEntity entity) {
        BuffDealerID = buffDealer?.UUID;
        BuffOwnerID = entity?.UUID;
        this.buffDealer = buffDealer;
        this.buffOwner = entity;
        OnInitialize();
    }
    
    public abstract void OnInitialize();
    

    public void OnStacked(IBuff buff) {
        if (buff is T tBuff) {
            OnStacked(tBuff);
        }
        else {
            Debug.LogError("Buff type mismatch for " + buff.GetType() + " and " + GetType() + " when stacking!");
        }
    }


    public virtual void OnAwake() {
        RemainingDuration = MaxDuration;
        TickTimer = TickInterval;
    }

    public abstract void OnStacked(T buff);


    public abstract void OnStart();


    public abstract BuffStatus OnTick();

    public abstract void OnEnd();
    

    public virtual void OnRecycled() {
        RemainingDuration = 0;
        BuffDealerID = null;
        BuffOwnerID = null;
        buffDealer = null;
        buffOwner = null;
        TickTimer = 0;
    }

    public bool IsRecycled { get; set; }
    public void RecycleToCache() {
        SafeObjectPool<T>.Singleton.Recycle(this as T);
    }
    
    public static T Allocate(IEntity buffDealer, IEntity entity) {
        T buff = SafeObjectPool<T>.Singleton.Allocate();
        buff.OnInitialize(buffDealer, entity);
        return buff;
    }
    
    public Buff() {
        
    }
    
    public Buff(IEntity buffDealer, IEntity entity) {
        OnInitialize(buffDealer, entity);
    }


}