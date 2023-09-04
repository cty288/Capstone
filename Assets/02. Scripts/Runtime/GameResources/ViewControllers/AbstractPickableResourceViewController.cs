using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Utilities;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;

public interface IPickableResourceViewController : IResourceViewController {
    
}

/// <summary>
/// For all resources on the ground
/// </summary>
[RequireComponent(typeof(TriggerCheck))]
//[RequireComponent(typeof(PoolableGameObject))]
public abstract class AbstractPickableResourceViewController<T> : AbstractResourceViewController<T>,
    IPickableResourceViewController where T : class, IResourceEntity, new() {
    protected TriggerCheck triggerCheck;
    protected ResLoader resLoader;
    protected PoolableGameObject poolable;
    
    
    protected void Awake() {
        poolable = GetComponent<PoolableGameObject>();
        triggerCheck = GetComponent<TriggerCheck>();
        resLoader = this.GetUtility<ResLoader>();
        if (poolable) {
            poolable.RegisterOnAllocateEvent(OnAwake);
            poolable.RegisterOnRecycledEvent(OnEnd);
        }
        else {
            OnAwake();
        }

    }
    protected virtual void OnAwake() {
        triggerCheck.OnEnter += OnEnter;
        triggerCheck.OnExit += OnExit;
    }
    protected virtual void OnEnd() {
        triggerCheck.OnEnter -= OnEnter;
        triggerCheck.OnExit -= OnExit;
    }



    private void OnExit(Collider obj) {
        
    }

    private void OnEnter(Collider obj) {
        
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        if (!poolable) {
            OnEnd();
        }
    }
}
