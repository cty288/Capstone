using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Events;
using _02._Scripts.Runtime.Levels.Sandstorm;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Enemies.Model;
using Runtime.Utilities;
using TMPro;
using UnityEngine;

public class RandomBossSpawning : AbstractMikroController<MainGame> {
	[SerializeField] private GameObject approachingPanel;
	[SerializeField] private TMP_Text text;

	private void Awake() {
		this.RegisterEvent<RandomBossEncounterEventTriggered>(OnRandomBossEncounter)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
	}

	private void OnRandomBossEncounter(RandomBossEncounterEventTriggered e) {
		Show(e.BossEntity);
	}
	
	
	private void Show(IEnemyEntity enemyEntity) {
		approachingPanel.gameObject.SetActive(true);
		text.text = Localization.GetFormat("BOSS_ENCOUNTER_EV_HINT", enemyEntity.GetDisplayName());
		this.Delay(4f, () => {
			approachingPanel.gameObject.SetActive(false);
		});
		
	}
	
}
