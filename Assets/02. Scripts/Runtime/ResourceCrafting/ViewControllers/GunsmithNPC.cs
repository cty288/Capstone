using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using Runtime.GameResources.Model.Base;
using Runtime.UI;
using UnityEngine;

public class GunsmithNPC : BaseNPC
{
	protected override void OnInteract() {
		base.OnInteract();
		if (!levelModel.BaseTutorialStatus.TalkedToGunsmith) {
			levelModel.BaseTutorialStatus.TalkedToGunsmith = true;
			NextConditionalDialogue();
		}
		else {
			OpenPanel();
		}
	}


	public void OpenPanel() {
		MainUI.Singleton.OpenOrGetClose
			<CraftingPanelViewController>(MainUI.Singleton, new CraftingPanelMsg() {
				category = ResearchCategory.WeaponAndParts
			}, false);
	}
}
