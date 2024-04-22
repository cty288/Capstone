using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NPCDialoguePanel : HintPanel, IPointerClickHandler {
	[SerializeField] private Button nextPageButton;
	[SerializeField] private TMP_Text dialogueText;
	[SerializeField] private Image icon;
	[SerializeField] private TMP_Text titleText;
	public override void OnInit() {
		base.OnInit();
		nextPageButton.onClick.AddListener(ShowNextMessage);
	}

	protected override void OnTerminateCurrentMessageGroup(bool isLastMessage) {
		
	}

	protected override void OnShowMessage() {
		HintMessage message = currentMessageGroup.messages[currentMessageIndex];
		dialogueText.text = message.message;
		icon.sprite = message.icon;
		titleText.text = message.title;
	}

	public void OnPointerClick(PointerEventData eventData) {
		ShowNextMessage();
	}
}
