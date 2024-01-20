using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Controls;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.UI;
using Runtime.UI.NameTags;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillPurchaseNPC : BaseNPC {
	protected override void OnInteract() {
		base.OnInteract();
		//MainUI.Singleton.OpenOrGetClose<SkillPurchaseUI>(MainUI.Singleton, null, false);
		MainUI.Singleton.OpenOrGetClose
			<CraftingPanelViewController>(MainUI.Singleton, new CraftingPanelMsg() {
				category = ResourceCategory.Skill
			}, false);
	}
}
