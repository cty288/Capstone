using System;
using System.Collections;
using System.Collections.Generic;
using Mikrocosmos.Controls;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Utilities;
using Polyglot;
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
    
    protected ResLoader resLoader;
    //protected PoolableGameObject poolable;
    

    [SerializeField] private string interactHintLocalizedKey = "interact_pick_up";
    protected override void Awake() {
        base.Awake();
        //poolable = GetComponent<PoolableGameObject>();
        resLoader = this.GetUtility<ResLoader>();
       
    }
}
