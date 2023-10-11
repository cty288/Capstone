using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Utilities;
using UnityEngine;

public abstract class EntityAttachedViewController<T> : AbstractMikroController<MainGame> where T : class, IEntity {
    protected IEntityViewController entityViewController;
    protected PoolableGameObject poolableGameObject;
    protected T boundEntity;

    protected virtual void Awake() {
        entityViewController = GetComponent<IEntityViewController>();
        poolableGameObject = GetComponent<PoolableGameObject>();
        
        poolableGameObject.RegisterOnAllocateEvent(OnAllocate);
        poolableGameObject.RegisterOnRecycledEvent(OnRecycle);
    }

    protected virtual void OnRecycle() {
        
    }

    private void OnAllocate() {
        entityViewController.RegisterOnEntityViewControllerInit(OnEntityInit)
            .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
    }

    private void OnEntityInit(IEntity e) {
        boundEntity = e as T;
        OnEntityFinishInit(boundEntity);
    }

    protected abstract void OnEntityFinishInit(T entity);
}
