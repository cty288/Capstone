using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;
using UnityEngine.UI;

public interface IInventoryResourceViewController : IResourceViewController {
    
}

public class CommonInventoryResourceViewController : AbstractResourceViewController<IResourceEntity>, 
    IInventoryResourceViewController {
    private Image icon;
    private ResLoader resLoader;
    protected override void Awake() {
        base.Awake();
        icon = transform.Find("ItemImage").GetComponent<Image>();
        resLoader = this.GetUtility<ResLoader>();
    }

    protected override IEntity OnBuildNewEntity() {
        return null;
    }

    protected override void OnEntityStart() {
        icon.sprite = resLoader.LoadSync<Sprite>($"{BoundEntity.EntityName}_Icon");
    }

    protected override void OnBindEntityProperty() {
        
    }
}
