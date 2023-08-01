using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Pool;
using UnityEngine;


public abstract class EntityBuilder<TBuilder, TEntity> : IPoolable 
    where TEntity : class, IEntity, new() 
    where TBuilder : EntityBuilder<TBuilder, TEntity>{
    protected virtual TEntity Entity { get; set; } = null;
    
    protected Action<TEntity> onEntityCreated = null;
    public EntityBuilder() {
       
    }
    
    public TBuilder RegisterOnEntityCreated(Action<TEntity> onCreated) {
        this.onEntityCreated += onCreated;
        return (TBuilder) this;
    }

    protected void CheckEntity() {
        if (Entity == null) {
            Entity = SafeObjectPool<TEntity>.Singleton.Allocate();
        }
    }

    public TBuilder FromConfig() {
        CheckEntity();
        Entity.LoadPropertyBaseValueFromConfig();
        return (TBuilder) this;
    }

    /// <summary>
    /// Override the property's base value and its modifier
    /// If the entity has multiple properties with the same name, every property will be overriden
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public TBuilder SetProperty<ValueType>(PropertyNameInfo propertyName, ValueType value, IPropertyDependencyModifier<ValueType> modifier = null) {
        CheckEntity();
        Entity.SetPropertyBaseValue(propertyName, value, modifier);
        return (TBuilder) this;
    }
    
    /// <summary>
    /// Override the property's modifier
    /// If the entity has multiple properties with the same name, every property will be overriden
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="modifier"></param>
    /// <typeparam name="ValueType"></typeparam>
    /// <returns></returns>
    public TBuilder SetModifier<ValueType>(PropertyNameInfo propertyName, IPropertyDependencyModifier<ValueType> modifier) {
        CheckEntity();
        Entity.SetPropertyModifier(propertyName, modifier);
        return (TBuilder) this;
    }

    public TEntity Build() {
        CheckEntity();
        TEntity ent = this.Entity;
        this.Entity = null;
        ent.OnAllocate();
        ent.Initialize();
        onEntityCreated?.Invoke(ent);
        RecycleToCache();
        return ent;
    }

    public void OnRecycled() {
        onEntityCreated = null;
    }

    public bool IsRecycled { get; set; }
    public abstract void RecycleToCache();
    
}
