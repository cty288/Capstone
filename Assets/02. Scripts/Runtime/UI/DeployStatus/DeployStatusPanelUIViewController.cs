using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Baits.Commands;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;

public class DeployStatusPanelUIViewController : AbstractMikroController<MainGame>
{
	private TMP_Text hint;
	private void Awake() {
		this.RegisterEvent<OnSetDeployStatusHint>(OnSetDeployStatusHint).UnRegisterWhenGameObjectDestroyed(gameObject);
		hint = transform.Find("Text").GetComponent<TMP_Text>();
		hint.text = "";
		hint.color = new Color(1, 1, 1, 0);
	}

	private void OnSetDeployStatusHint(OnSetDeployStatusHint e) {
		hint.text = e.hint;
		if(String.IsNullOrEmpty(e.hint)) {
			hint.DOFade(0, 0.2f);
		}
		else {
			hint.DOFade(1, 0.2f);
		}
		
	}
}
