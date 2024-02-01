using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Utilities;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.UI.NameTags;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public class GlobalLevelManager : MonoMikroSingleton<GlobalLevelManager>, IController {
		[SerializeField] protected List<GameObject> levels = new List<GameObject>();
		[SerializeField] protected GameObject baseLevel;
		[SerializeField] private GameObject directStartContainer;
		[SerializeField] private int directStartLevelNumber = 1;
		

		private Dictionary<string, GameObject> globalPrefabList = null;
		private IEntity directStartLevel = null;

		private GameObject currentLevelGo;
		private ILevelViewController currentLevelViewController;

		public ILevelViewController CurrentLevelViewController => currentLevelViewController;
		private ILevelModel levelModel;
		private void Awake() {
			
			levelModel = this.GetModel<ILevelModel>();
			//levels.Shuffle();
			levels.Insert(0, baseLevel);
			this.RegisterEvent<OnTryToSwitchUnSpawnedLevel>(OnTryToSwitchUnSpawnedLevel).UnRegisterWhenGameObjectDestroyed(gameObject);
		}
		

		private void Start() {
			
			levelModel.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
			if (directStartContainer.transform.childCount > 0) {
				GameObject level = directStartContainer.transform.GetChild(0).gameObject;
				if (level.activeInHierarchy) {
					directStartLevel = AddLevel(level, directStartLevelNumber);
					level.gameObject.SetActive(false);
				}
			}
			levelModel.SwitchToLevel(levelModel.CurrentLevelCount.Value);
		}

		public GameObject GetEnemyPrefab(string prefabName) {
			if (globalPrefabList == null) {
				globalPrefabList = new Dictionary<string, GameObject>();
				FetchPrefab();
			}

			return globalPrefabList[prefabName];
		}

		private void FetchPrefab() {
			foreach (GameObject level in levels) {
				ILevelViewController levelViewController = level.GetComponent<ILevelViewController>();
				foreach (GameObject enemy in levelViewController.Enemies) {
					globalPrefabList.TryAdd(enemy.name, enemy);
				}
			}
		}

		private void OnCurrentLevelChanged(ILevelEntity oldLevel, ILevelEntity newLevel) {
			if (currentLevelGo) {
				currentLevelGo.GetComponent<ILevelViewController>().OnExitLevel();
				oldLevel?.OnLevelExit();
				Destroy(currentLevelGo, 0.1f);
			}
			
			HUDManager.Singleton.ClearAll();
			HUDManagerUI.Singleton.ClearAll();
			currentLevelViewController = null;
			if (newLevel == null) {
				return;
			}

			int levelCount = newLevel.GetCurrentLevelCount();
			GameObject level = levels[levelCount];
			GameObject spawnedLevel = null;
			if (directStartLevel != null && directStartLevel == newLevel) {
				spawnedLevel = directStartContainer.transform.GetChild(0).gameObject;
				spawnedLevel.gameObject.SetActive(true);
			}
			else {
				spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
			}
			
			ILevelViewController levelViewController = spawnedLevel.GetComponent<ILevelViewController>();
			currentLevelViewController = levelViewController;
			levelViewController.SetLevelNumber(levelCount);
			
			levelViewController.InitWithID(newLevel.UUID);
			levelViewController.Init();
			
			currentLevelGo = spawnedLevel;
		}

		private void OnTryToSwitchUnSpawnedLevel(OnTryToSwitchUnSpawnedLevel e) {
			int levelCount = e.LevelNumber;
			if (levelCount >= levels.Count) {
				return;
			}
			AddLevel(levels[levelCount], levelCount);
		}
		
		private ILevelEntity AddLevel(GameObject levelPrefab, int levelCount) {
			
			GameObject level = levelPrefab;
			ILevelViewController levelViewController = level.GetComponent<ILevelViewController>();
			ILevelEntity entity = levelViewController.OnBuildNewLevel(levelCount);
			levelModel.AddLevel(entity);
			return entity;
		}
		
	
		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
		
		
	}
}