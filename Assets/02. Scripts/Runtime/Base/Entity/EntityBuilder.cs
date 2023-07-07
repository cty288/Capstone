using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Pool;
using NHibernate.Tuple;
using UnityEngine;

public abstract class EntityBuilder<T> : IPoolable where T : class, IEntity, new()
{
    protected T entity = null;

    public EntityBuilder() {
       
    }
    
    protected void CheckEntity() {
        if (entity == null) {
            entity = SafeObjectPool<T>.Singleton.Allocate();
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
        entity.SetPropertyBaseValue(propertyName, value, modifier);
        return this;
    }
    
    public EntityBuilder<T> SetModifier<ValueType>(PropertyName propertyName, IPropertyDependencyModifier<ValueType> modifier) {
        CheckEntity();
        entity.SetPropertyModifier(propertyName, modifier);
        return this;
    }

    public T Build() {
        CheckEntity();
        T ent = this.entity;
        this.entity = null;
        ent.OnAllocate();
        ent.Initialize();
        RecycleToCache();
        return ent;
    }

    public void OnRecycled() {
        
    }

    public bool IsRecycled { get; set; }
    public abstract void RecycleToCache();
    
}
