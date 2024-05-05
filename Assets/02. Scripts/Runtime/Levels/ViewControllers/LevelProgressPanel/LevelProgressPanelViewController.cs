using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Levels.Systems;
using _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial;
using _02._Scripts.Runtime.PlayerTasks;
using DG.Tweening;
using Framework;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Event;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

public struct OnClearTask {
	public PlayerTask Task;
}

public class LevelProgressPanelViewController : AbstractMikroController<MainGame> {
	private ILevelSystem levelSystem;
	private ILevelModel levelModel;
	private Slider explorationProgressSlider;
	private RectTransform taskPanel;
	private float totalExplorationValue;
	private IPlayerTaskSystem playerTaskSystem;

	private Dictionary<PlayerTask, TaskElementViewController> taskElements =
		new Dictionary<PlayerTask, TaskElementViewController>();
	
	

	[SerializeField] private GameObject taskElementPrefab;
	private void Awake() {
		levelSystem = this.GetSystem<ILevelSystem>();
		levelModel = this.GetModel<ILevelModel>();

		explorationProgressSlider = transform.Find("ExplorationProgress").GetComponent<Slider>();
		taskPanel = transform.Find("TaskPanel").GetComponent<RectTransform>();

		levelModel.CurrentLevel.RegisterWithInitValue(OnLevelChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
		playerTaskSystem = this.GetSystem<IPlayerTaskSystem>();

		this.RegisterEvent<OnAddPlayerTask>(OnAddPlayerTask).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		this.RegisterEvent<OnTaskCompleted>(OnTaskCompleted).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		this.RegisterEvent<OnClearTaskPanel>(OnClearTaskPanel).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		this.RegisterEvent<OnClearTask>(OnClearTask).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		foreach (PlayerTask playerTask in playerTaskSystem.GetAllTasks()) {
			SpawnTask(playerTask);
		}
	}



	private void OnClearTaskPanel(OnClearTaskPanel obj) {
		ClearTasks();
	}

	private void OnTaskCompleted(OnTaskCompleted e) {
		if (taskElements.TryGetValue(e.Task, out TaskElementViewController taskElementViewController)) {
			AudioSystem.Singleton.Play2DSound("checkmark");
			taskElementViewController.SetCompleted(true);
		}
	}

	private void OnAddPlayerTask(OnAddPlayerTask e) {
		SpawnTask(e.Task);
	}

	private void OnLevelExitSatisfied(bool arg1, bool condition) {
		
	}

	private void OnCurrentLevelExitContitionSatisfied(OnCurrentLevelExitContitionSatisfied e) {
		/*if (taskElements.TryGetValue(e.Condition, out TaskElementViewController taskElementViewController)) {
			taskElementViewController.SetCompleted(e.Condition.IsSatisfied());
		}*/
		
	}

	private void OnLevelChanged(ILevelEntity oldLevel, ILevelEntity newLevel) {
		DisableExplorationUIs();
		UnRegisterExplorationStatus(oldLevel);
		RegisterExplorationStatus(newLevel);
	}


	private TaskElementViewController SpawnTask(PlayerTask playerTask) {
		GameObject taskObj = Instantiate(taskElementPrefab, taskPanel);
		TaskElementViewController taskElementViewController = taskObj.GetComponent<TaskElementViewController>();
		taskElementViewController.Init(playerTask.GetDescription());
		taskElements.Add(playerTask, taskElementViewController);
		StartCoroutine(RebuildLayout());
		return taskElementViewController;
	}

	private void Update() {
		List<PlayerTask> tasks = taskElements.Keys.ToList();
		foreach (PlayerTask task in tasks) {
			TaskElementViewController taskElementViewController = taskElements[task];
			taskElementViewController.SetDescription(task.GetDescription());
		}
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
		
		if(levelEntity.LevelExitConditions.TryGetValue(typeof(LevelExplorationCondition), out PlayerTask val)){
			LevelExplorationCondition condition = val as LevelExplorationCondition;
			condition?.CurrentValue.UnRegisterOnValueChanged(OnExplorationValueChanged);
		}
	}
	
	private void RegisterExplorationStatus(ILevelEntity levelEntity) {
		if (levelEntity == null) {
			explorationProgressSlider.gameObject.SetActive(false);
			return;
		}
		
		if(levelEntity.LevelExitConditions.TryGetValue(typeof(LevelExplorationCondition), out PlayerTask val)){
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

	private void DisableExplorationUIs() {
		
		explorationProgressSlider.gameObject.SetActive(false);
		explorationProgressSlider.value = 0;
		

		/*for (int i = 0; i < taskPanel.childCount; i++) {
			Destroy(taskPanel.GetChild(i).gameObject);
		}
		*/
	}

	public void ClearTasks() {
		for (int i = 0; i < taskPanel.childCount; i++) {
			Destroy(taskPanel.GetChild(i).gameObject);
		}
		taskElements.Clear();
	}
	
	private void OnClearTask(OnClearTask e) {
		PlayerTask task = e.Task;
		if (taskElements.TryGetValue(task, out TaskElementViewController taskElementViewController)) {
			taskElements.Remove(task);
			Destroy(taskElementViewController.gameObject);
		}
	}
}
