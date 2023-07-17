using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Pool;
using UnityEngine;


public abstract class EntityBuilder<T> : IPoolable where T : class, IEntity, new()
{
    protected virtual T Entity { get; set; } = null;
    
    protected Action<T> onEntityCreated = null;
    public EntityBuilder() {
       
    }
    
    public EntityBuilder<T> RegisterOnEntityCreated(Action<T> onCreated) {
        this.onEntityCreated += onCreated;
        return this;
    }

    protected void CheckEntity() {
        if (Entity == null) {
            Entity = SafeObjectPool<T>.Singleton.Allocate();
        }
    }

    public EntityBuilder<T> FromConfig() {
        CheckEntity();
        return this;
    }

    /// <summary>
    /// Override the property's base value and its modifier
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public EntityBuilder<T> SetProperty<ValueType>(PropertyName propertyName, ValueType value, IPropertyDependencyModifier<ValueType> modifier = null) {
        CheckEntity();
        Entity.SetPropertyBaseValue(propertyName, value, modifier);
        return this;
    }
    
    public EntityBuilder<T> SetModifier<ValueType>(PropertyName propertyName, IPropertyDependencyModifier<ValueType> modifier) {
        CheckEntity();
        Entity.SetPropertyModifier(propertyName, modifier);
        return this;
    }

    public T Build() {
        CheckEntity();
        T ent = this.Entity;
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
