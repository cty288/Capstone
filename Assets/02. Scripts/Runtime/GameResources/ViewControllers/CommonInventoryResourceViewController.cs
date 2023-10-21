using System;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.GameResources.ViewControllers {
    public interface IInventoryResourceViewController : IResourceViewController {
    
    }

    public class CommonInventoryResourceViewController : AbstractResourceViewController<IResourceEntity>, 
        IInventoryResourceViewController {
        private Image icon;
        protected override void Awake() {
            base.Awake();
            icon = transform.Find("ItemImage").GetComponent<Image>();
        }

        protected override IEntity OnBuildNewEntity() {
            return null;
        }

        protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;

        protected override void OnEntityStart() {
            icon.sprite = InventorySpriteFactory.Singleton.GetSprite(BoundEntity.IconSpriteName);
        }

        protected override void OnBindEntityProperty() {
        
        }

        
    }
}