using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitConfirmationPanel : AbstractPanel, IController, IGameUIPanel {
	[SerializeField] private Button yesButton;
	[SerializeField] private Button noButton;
	public override void OnInit() {
		yesButton.onClick.AddListener(OnYesButtonClicked);
		noButton.onClick.AddListener(OnNoButtonClicked);
	}

	private void OnNoButtonClicked() {
		MainUI.Singleton.GetAndClose(this);
	}

	private void OnYesButtonClicked() {
		var parent = Parent;
		MainUI.Singleton.GetAndClose(this);
		MainUI.Singleton.GetAndClose(parent);
		MainGame.ResetArchitecture();
		SceneManager.LoadScene(1);
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
