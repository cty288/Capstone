using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.UI;
using UnityEngine;

public struct CraftingPanelMsg : UIMsg {
	public ResearchCategory category;
}

public class CraftingPanelViewController : SwitchableRootPanel {
	private CraftingPanelMsg msg;

	[SerializeField] private ResearchPanelViewController researchPanelViewController;
	[SerializeField] private BuildPanelViewController buildPanelViewController;
	
	public override void OnOpen(UIMsg msg) {
		this.msg = (CraftingPanelMsg) msg;
		base.OnOpen(msg);
	}

	protected override void OnSubpanelSelected(SwitchableSubPanel panel, int index) {
		
		base.OnSubpanelSelected(panel, index);
		if (panel == researchPanelViewController) {
			researchPanelViewController.OnSetResourceCategory(this.msg.category);
		}else if (panel == buildPanelViewController) {
			buildPanelViewController.OnSetResourceCategory(this.msg.category);
		}
	}
}
