using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using Runtime.GameResources.Model.Base;
using Runtime.UI;
using UnityEngine;

public class BaseInventoryNPC : BaseNPC
{
    protected override void OnInteract() {
        base.OnInteract();
        //MainUI.Singleton.OpenOrGetClose<SkillPurchaseUI>(MainUI.Singleton, null, false);
        MainUI.Singleton.OpenOrGetClose
            <BaseInventoryPanel>(MainUI.Singleton, new BaseInventoryPanelMsg() {
                category = ResourceCategory.RawMaterial
            }, false);
    }
}
