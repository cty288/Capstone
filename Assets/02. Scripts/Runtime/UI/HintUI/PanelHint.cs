using System.Collections;
using System.Collections.Generic;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PanelHint : HintPanel {
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private TMP_Text messageText;
	[SerializeField] private Image icon;
	[SerializeField] private Button nextPageButton;
	[SerializeField] private Button closeButton;
	[SerializeField] private Button lastPageButton;
	public override void OnInit() {
		base.OnInit();
		nextPageButton.onClick.AddListener(ShowNextMessage);
		closeButton.onClick.AddListener(() => {
			MainUI.Singleton.GetAndClose(this);
		});
		lastPageButton.onClick.AddListener(ShowLastMessage);
	}

	protected override void OnTerminateCurrentMessageGroup(bool isLastMessage) {
		
	}

	protected override void OnShowMessage() {
		HintMessage message = currentMessageGroup.messages[currentMessageIndex];
		titleText.text = message.title;
		messageText.text = message.message;
		icon.sprite = message.icon;
		lastPageButton.gameObject.SetActive(currentMessageIndex > 0);
	}
	
}
