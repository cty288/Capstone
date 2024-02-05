using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Player.Commands;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Polyglot;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Player;
using Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDieCanvas : AbstractMikroController<MainGame> {
	private GameObject panel;
	private TMP_Text deathReasonText;
	private Button restartButton;
	private void Awake() {
		this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
		panel = transform.Find("Panel").gameObject;
		deathReasonText = panel.transform.Find("DeathReasonText").GetComponent<TMP_Text>();
		restartButton = panel.transform.Find("BackButton").GetComponent<Button>();

		restartButton.onClick.AddListener(OnRestartClicked);
	}

	private void OnRestartClicked() {
		ClientInput.Singleton.EnablePlayerMaps();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
		panel.SetActive(false);

		this.SendCommand<PlayerRespawnCommand>();

	}

	private void OnPlayerDie(OnPlayerDie e) {
		ClientInput.Singleton.EnableUIMaps();
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
		deathReasonText.text = "";
		
		panel.SetActive(true);

		ICanDealDamage damageDealer = e.damageDealer;
		if (damageDealer == null) {
			return;
		}

		ICanDealDamage rootEntity = damageDealer.GetRootDamageDealer();
		if (rootEntity == null) {
			return;
		}

		if (rootEntity is IPlayerEntity) {
			deathReasonText.text = Localization.Get("DIE_PAGE_REASON_2");
		}
		else if(rootEntity is IEntity entity) {
			deathReasonText.text = Localization.GetFormat("DIE_PAGE_REASON_1", entity.GetDisplayName());
		}
	}
}
