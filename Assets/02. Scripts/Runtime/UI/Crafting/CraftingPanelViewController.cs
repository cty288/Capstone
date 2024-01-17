using System.Collections;
using System.Collections.Generic;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.UI;
using UnityEngine;

public struct CraftingPanelMsg : UIMsg {
	public ResourceCategory category;
}

public class CraftingPanelViewController : SwitchableRootPanel {
	private CraftingPanelMsg msg;

	[SerializeField] private ResearchPanelViewController researchPanelViewController;
	
	public override void OnOpen(UIMsg msg) {
		this.msg = (CraftingPanelMsg) msg;
		base.OnOpen(msg);
	}

	protected override void OnSubpanelSelected(SwitchableSubPanel panel) {
		
		base.OnSubpanelSelected(panel);
		if (panel == researchPanelViewController) {
			researchPanelViewController.OnSetResourceCategory(this.msg.category);
		}
	}
}
