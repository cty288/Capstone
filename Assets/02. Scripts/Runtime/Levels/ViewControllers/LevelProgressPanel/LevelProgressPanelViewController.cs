using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Levels.Systems;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressPanelViewController : AbstractMikroController<MainGame> {
	private ILevelSystem levelSystem;
	private ILevelModel levelModel;
	private Slider explorationProgressSlider;
	private RectTransform taskPanel;
	private float totalExplorationValue;

	private Dictionary<LevelExitCondition, TaskElementViewController> taskElements =
		new Dictionary<LevelExitCondition, TaskElementViewController>(); 
	private TaskElementViewController enterExitTaskElement;

	[SerializeField] private GameObject taskElementPrefab;
	private void Awake() {
		levelSystem = this.GetSystem<ILevelSystem>();
		levelModel = this.GetModel<ILevelModel>();

		explorationProgressSlider = transform.Find("ExplorationProgress").GetComponent<Slider>();
		taskPanel = transform.Find("TaskPanel").GetComponent<RectTransform>();

		levelModel.CurrentLevel.RegisterWithInitValue(OnLevelChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
		levelSystem.IsLevelExitSatisfied.RegisterOnValueChanged(OnLevelExitSatisfied)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);

		this.RegisterEvent<OnCurrentLevelExitContitionSatisfied>(OnCurrentLevelExitContitionSatisfied)
			.UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnLevelExitSatisfied(bool arg1, bool condition) {
		if (condition) {
			enterExitTaskElement = SpawnTask(new EnterExitCondition());
		}
		else {
			if (enterExitTaskElement) {
				Destroy(enterExitTaskElement.gameObject);
			}
		}
	}

	private void OnCurrentLevelExitContitionSatisfied(OnCurrentLevelExitContitionSatisfied e) {
		if (taskElements.TryGetValue(e.Condition, out TaskElementViewController taskElementViewController)) {
			taskElementViewController.SetCompleted(e.Condition.IsSatisfied());
		}
		
	}

	private void OnLevelChanged(ILevelEntity oldLevel, ILevelEntity newLevel) {
		DisableAllUIs();
		UnRegisterExplorationStatus(oldLevel);
		SpawnLevelTasks(newLevel);
		RegisterExplorationStatus(newLevel);
	}

	private void SpawnLevelTasks(ILevelEntity newLevel) {
		if (newLevel == null) {
			return;
		}

		foreach (LevelExitCondition levelExitCondition in newLevel.LevelExitConditions.Values) {
			SpawnTask(levelExitCondition);
		}

		//StartCoroutine(RebuildLayout());
	}
	
	private TaskElementViewController SpawnTask(LevelExitCondition levelExitCondition) {
		GameObject taskObj = Instantiate(taskElementPrefab, taskPanel);
		TaskElementViewController taskElementViewController = taskObj.GetComponent<TaskElementViewController>();
		taskElementViewController.Init(levelExitCondition.GetDescription());
		taskElements.Add(levelExitCondition, taskElementViewController);
		StartCoroutine(RebuildLayout());
		return taskElementViewController;
	}
	
	private IEnumerator RebuildLayout() {
		LayoutRebuilder.ForceRebuildLayoutImmediate(taskPanel);
		yield return new WaitForEndOfFrame();
		LayoutRebuilder.ForceRebuildLayoutImmediate(taskPanel);
	}

	private void UnRegisterExplorationStatus(ILevelEntity levelEntity) {
		if (levelEntity == null) {
			return;
		}
		
		if(levelEntity.LevelExitConditions.TryGetValue(typeof(LevelExplorationCondition), out LevelExitCondition val)){
			LevelExplorationCondition condition = val as LevelExplorationCondition;
			condition?.CurrentValue.UnRegisterOnValueChanged(OnExplorationValueChanged);
		}
	}
	
	private void RegisterExplorationStatus(ILevelEntity levelEntity) {
		if (levelEntity == null) {
			explorationProgressSlider.gameObject.SetActive(false);
			return;
		}
		
		if(levelEntity.LevelExitConditions.TryGetValue(typeof(LevelExplorationCondition), out LevelExitCondition val)){
			explorationProgressSlider.gameObject.SetActive(true);
			LevelExplorationCondition condition = val as LevelExplorationCondition;
			
			totalExplorationValue = condition?.TotalValue ?? float.MaxValue;
			condition?.CurrentValue.RegisterWithInitValue(OnExplorationValueChanged)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			
		}
		else {
			explorationProgressSlider.gameObject.SetActive(false);
		}
	}

	private void OnExplorationValueChanged(float arg1, float newVal) {
		explorationProgressSlider.DOValue(newVal / totalExplorationValue, 0.3f);
	}

	private void DisableAllUIs() {
		taskElements.Clear();
		explorationProgressSlider.gameObject.SetActive(false);
		explorationProgressSlider.value = 0;
		

		for (int i = 0; i < taskPanel.childCount; i++) {
			Destroy(taskPanel.GetChild(i).gameObject);
		}

		enterExitTaskElement = null;
	}
}
