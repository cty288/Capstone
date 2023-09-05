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
    protected TriggerCheck triggerCheck;
    protected ResLoader resLoader;
    //protected PoolableGameObject poolable;
    protected InteractHintCanvas hintCanvas;

    [SerializeField] private string interactHintLocalizedKey = "interact_pick_up";
    protected virtual void Awake() {
        //poolable = GetComponent<PoolableGameObject>();
        triggerCheck = GetComponent<TriggerCheck>();
        resLoader = this.GetUtility<ResLoader>();
        triggerCheck.OnEnter += OnEnter;
        triggerCheck.OnExit += OnExit;
        hintCanvas = GetComponentInChildren<InteractHintCanvas>(true);
        TryHideHint();
    }
    
    private void OnEnter(Collider other) {
        if(other.CompareTag("Player")) {
            TryShowHint();
        }
    }
    private void OnExit(Collider other) {
        if(other.CompareTag("Player")) {
            TryHideHint();
            
        }
    }
    
    private void TryShowHint() {
        if (hintCanvas && BoundEntity.Pickable.Value) {
            hintCanvas.Show();
            hintCanvas.SetHint(ClientInput.Singleton.FindActionInPlayerActionMap("Interact"),
                Localization.Get(interactHintLocalizedKey));
            hintCanvas.SetName(BoundEntity.GetDisplayName());
        }
    }
    
    private void TryHideHint() {
        if (hintCanvas) {
            hintCanvas.Hide();
        }
    }
    
    protected override void OnEntityStart() {
        BoundEntity.Pickable.RegisterWithInitValue(OnPickableChanged)
            .UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnPickableChanged(bool oldValue, bool newValue) {
        if(newValue && triggerCheck.Triggered) {
            TryShowHint();
        }
        
        if(!newValue && triggerCheck.Triggered) {
            TryHideHint();
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        triggerCheck.OnEnter -= OnEnter;
        triggerCheck.OnExit -= OnExit;
    }
}
