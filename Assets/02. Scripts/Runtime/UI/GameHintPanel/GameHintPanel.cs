using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;

public struct OnShowGameHint {
	public float duration;
	public string text;
}
public class GameHintPanel : AbstractMikroController<MainGame> {
	private TMP_Text hintText;
	private void Awake() {
		hintText = transform.Find("Text").GetComponent<TMP_Text>();
		this.RegisterEvent<OnShowGameHint>(OnShowGameHint)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnShowGameHint(OnShowGameHint e) {
		hintText.DOKill();
		hintText.text = e.text;
		hintText.DOFade(1, 0.5f).OnComplete(() => {
			hintText.DOFade(0, 0.5f).SetDelay(e.duration);
		});
	}
}
