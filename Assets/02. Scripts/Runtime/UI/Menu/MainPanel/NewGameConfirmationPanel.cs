using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.UIKit;
using UnityEngine;
using UnityEngine.UI;

public class NewGameConfirmationPanel : AbstractPanel {
	[SerializeField] private Button yesButton;
	[SerializeField] private Button noButton;
	public override void OnInit() {
		yesButton.onClick.AddListener(OnYesButtonClicked);
		noButton.onClick.AddListener(OnNoButtonClicked);
	}

	private void OnNoButtonClicked() {
		UIManager.Singleton.ClosePanel(this);
	}

	private void OnYesButtonClicked() {
		MainGame.ClearAllSaves();
		MainGame.ResetArchitecture();
		UIManager.Singleton.ClosePanel(this);
		MenuMainPanel.EnterGame();
	}

	public override void OnOpen(UIMsg msg) {
		
	}

	public override void OnClosed() {
		
	}
}
