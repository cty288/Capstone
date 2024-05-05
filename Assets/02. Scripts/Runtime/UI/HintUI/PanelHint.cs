using System.Collections;
using System.Collections.Generic;
using MikroFramework.AudioKit;
using Polyglot;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

public class PanelHint : HintPanel {
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private TMP_Text messageText;
	//[SerializeField] private Image icon;
	
	[SerializeField] private RenderTexture panelRenderTexture;
	[SerializeField] private VideoPlayer panelVideoPlayer;
	[SerializeField] private RawImage panelRawImage;
	
	[SerializeField] private Button nextPageButton;
	[SerializeField] private Button closeButton;
	[SerializeField] private Button lastPageButton;
	public override void OnInit() {
		base.OnInit();
		nextPageButton.onClick.AddListener(ShowNextMessage);
		closeButton.onClick.AddListener(() => {
			AudioSystem.Singleton.Play2DSound("interact_button");
			var lastHintMessage = currentMessageGroup.messages[^1];
			if (lastHintMessage != null && lastHintMessage.callback != null) {
				lastHintMessage.callback.Invoke();
			}
			
			MainUI.Singleton.GetAndClose(this);
		});
		lastPageButton.onClick.AddListener(ShowLastMessage);
	}

	protected override void OnTerminateCurrentMessageGroup(bool isLastMessage) {
		
	}

	protected override void OnShowMessage() {
		AudioSystem.Singleton.Play2DSound("arrow_click");
		HintMessage message = currentMessageGroup.messages[currentMessageIndex];
		titleText.text = Localization.Get(message.titleLocalizedKey);
		messageText.text = GetLocalizedText(message);
		//icon.sprite = message.icon;
		panelVideoPlayer.Stop();

		if (message.panelImage != null) {
			panelRawImage.texture = message.panelImage;
		}else if (message.panelVideo != null) {
			panelRawImage.texture = panelRenderTexture;
			panelVideoPlayer.clip = message.panelVideo;
			panelVideoPlayer.Play();
		}
		lastPageButton.gameObject.SetActive(currentMessageIndex > 0);
	}
	
}
