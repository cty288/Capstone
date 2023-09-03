using System;
using System.Collections;
using System.Collections.Generic;
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
public abstract class AbstractPickableResourceViewController<T> : AbstractResourceViewController<T>, 
    IPickableResourceViewController where T : class, IResourceEntity, new() {
    protected TriggerCheck triggerCheck;
    protected virtual void Awake() {
        triggerCheck = GetComponent<TriggerCheck>();
        
    }
}
