using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using AYellowpaper.SerializedCollections;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.UI;
using UnityEngine;
public struct BaseInventoryPanelMsg : UIMsg {
	public ResourceCategory category;
}
public class BaseInventoryPanel : SwitchableRootPanel {
	private BaseInventoryPanelMsg msg;

	[SerializedDictionary("index", "category")] 
	[SerializeField]
	private SerializedDictionary<int, ResourceCategory> categoryToIndex;

	[SerializeField] private BaseInventorySubPanel subPanel;


	public override void OnOpen(UIMsg msg) {
		this.msg = (BaseInventoryPanelMsg) msg;
		base.OnOpen(msg);
	}

	protected override void OnSubpanelSelected(SwitchableSubPanel panel, int index) {
		base.OnSubpanelSelected(panel, index);
		ResourceCategory category = categoryToIndex[index];
		subPanel.OnSetResourceCategory(category);
	}
}
