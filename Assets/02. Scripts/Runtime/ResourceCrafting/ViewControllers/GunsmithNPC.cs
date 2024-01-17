using System.Collections;
using System.Collections.Generic;
using Runtime.GameResources.Model.Base;
using Runtime.UI;
using UnityEngine;

public class GunsmithNPC : BaseNPC
{
	protected override void OnInteract() {
		base.OnInteract();
		MainUI.Singleton.OpenOrGetClose
			<CraftingPanelViewController>(MainUI.Singleton, new CraftingPanelMsg() {
				category = ResourceCategory.WeaponParts
			}, false);
	}
}
