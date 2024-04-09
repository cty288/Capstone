using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePausePanel  : AbstractPanelContainer, IController, IGameUIPanel {
	[SerializeField] private Button continueButton;
	[SerializeField] private Button exitButton;
	public override void OnInit() {
		continueButton.onClick.AddListener(OnContinueButtonClicked);
		exitButton.onClick.AddListener(OnExitButtonClicked);
	}

	private void OnExitButtonClicked() {
		MainUI.Singleton.Open<ExitConfirmationPanel>(this, null, true, true);
	}

	private void OnContinueButtonClicked() {
		MainUI.Singleton.GetAndClose(this);
		
	}

	public override void OnOpen(UIMsg msg) {
		
	}

	public override void OnClosed() {
		
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}

	public IPanel GetClosePanel() {
		IPanel openedChild = GetTopChild();
		if (openedChild != null) {
			return openedChild;
		}
            
		return this;
	}
}
