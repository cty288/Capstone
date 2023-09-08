using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mikrocosmos.Controls;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using MikroFramework.Utilities;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;
using Sequence = MikroFramework.ActionKit.Sequence;

public interface IPickableResourceViewController : IResourceViewController {
    
}

/// <summary>
/// For all resources on the ground
/// </summary>
//[RequireComponent(typeof(TriggerCheck))]
//[RequireComponent(typeof(PoolableGameObject))]
public abstract class AbstractPickableResourceViewController<T> : AbstractResourceViewController<T>,
    IPickableResourceViewController where T : class, IResourceEntity, new() {
    
    protected ResLoader resLoader;
    //protected PoolableGameObject poolable;
    protected IInventorySystem inventorySystem;
    protected bool isAbsorbing = false;
    protected override void Awake() {
        base.Awake();
        //poolable = GetComponent<PoolableGameObject>();
        resLoader = this.GetUtility<ResLoader>();
        inventorySystem = this.GetSystem<IInventorySystem>();
    }

    public override void OnPlayerEnterInteractiveZone(GameObject player, PlayerInteractiveZone zone) {
        base.OnPlayerEnterInteractiveZone(player, zone);
        HandleAbsorb(player, zone);
    }

    private void HandleAbsorb(GameObject player, PlayerInteractiveZone zone) {
        if (!player || !Camera.main || isAbsorbing) return;
        if(inventorySystem.AddItem(BoundEntity)) {
            isAbsorbing = true;
            transform.DOMoveInTargetLocalSpace(Camera.main.transform, Vector3.zero, 0.2f).
                SetEase(Ease.Linear)
                .OnComplete(() => {
                    Destroy(this.gameObject);
                });
        }
        else {
            //did not add successfully, will wait until the player has enough space, use Until Action
            //TODO: test it
            Sequence.Allocate()
                .AddAction(
                    UntilAction.Allocate(() => inventorySystem.CanPlaceItem(BoundEntity, out int index) || zone.IsInZone(this) || !this))
                .AddAction(CallbackAction.Allocate(() => {
                    if (this && zone.IsInZone(this)) {
                        HandleAbsorb(player, zone);
                    }
                })).Execute();
        }
    }
}

