using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleHint : HintPanel {
	[SerializeField] private TMP_Text subtitleText;
	protected override void OnTerminateCurrentMessageGroup(bool isLastMessage) {
		
	}

	protected override void OnShowMessage() {
		HintMessage message = currentMessageGroup.messages[currentMessageIndex];
		subtitleText.text = message.message;
		float duration = message.duration;
		StartCoroutine(SubtitleWait(duration));
		StartCoroutine(RebuildLayout());
	}
	
	private IEnumerator RebuildLayout() {
		RectTransform rectTransform = transform.GetChild(0).GetComponent<RectTransform>();
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
	}
	
	private IEnumerator SubtitleWait(float duration) {
		yield return new WaitForSeconds(duration);
		if (currentMessageGroup == null) {
			yield break;
		}
		ShowNextMessage();
	}
}
