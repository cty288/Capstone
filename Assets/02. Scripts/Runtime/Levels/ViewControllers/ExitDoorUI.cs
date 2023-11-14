using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Commands;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.UI;
using UnityEngine;
using UnityEngine.UI;

public class ExitDoorUI : AbstractPanelContainer, IController, IGameUIPanel {
	private Button baseButton;
	private Button nextLevelButton;
	
	public override void OnInit() {
		baseButton = transform.Find("BaseButton").GetComponent<Button>();
		nextLevelButton = transform.Find("NextLevelButton").GetComponent<Button>();
		
		baseButton.onClick.AddListener(OnBaseButtonClicked);
		nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
	}

	private void OnNextLevelButtonClicked() {
		this.SendCommand<NextLevelCommand>(NextLevelCommand.Allocate());
		MainUI.Singleton.OpenOrGetClose<ExitDoorUI>(MainUI.Singleton, null);
	}

	private void OnBaseButtonClicked() {
		this.SendCommand<BackToBaseCommand>(BackToBaseCommand.Allocate());
		MainUI.Singleton.OpenOrGetClose<ExitDoorUI>(MainUI.Singleton, null);
	}

	public override void OnOpen(UIMsg msg) {
		
	}

	public override void OnClosed() {
		
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}

	public IPanel GetClosePanel() {
		return this;
	}
}
