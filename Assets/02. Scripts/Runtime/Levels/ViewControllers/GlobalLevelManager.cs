﻿using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Utilities;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Singletons;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public class GlobalLevelManager : MonoMikroSingleton<GlobalLevelManager>, IController {
		[SerializeField] protected List<GameObject> levels = new List<GameObject>();
		[SerializeField] protected GameObject baseLevel;

		private Dictionary<string, GameObject> globalPrefabList = null;

		private GameObject currentLevelGo;
		private ILevelModel levelModel;
		private void Awake() {
			levelModel = this.GetModel<ILevelModel>();
			//levels.Shuffle();
			levels.Insert(0, baseLevel);
			this.RegisterEvent<OnTryToSwitchUnSpawnedLevel>(OnTryToSwitchUnSpawnedLevel).UnRegisterWhenGameObjectDestroyed(gameObject);
		}
		

		private void Start() {
			levelModel.CurrentLevel.RegisterWithInitValue(OnCurrentLevelChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
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
				Destroy(currentLevelGo);
			}
			
			if (newLevel == null) {
				return;
			}

			int levelCount = newLevel.GetCurrentLevelCount();
			GameObject level = levels[levelCount];
			
			GameObject spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
			ILevelViewController levelViewController = spawnedLevel.GetComponent<ILevelViewController>();
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
			
			GameObject level = levels[levelCount];
			ILevelViewController levelViewController = level.GetComponent<ILevelViewController>();
			ILevelEntity entity = levelViewController.OnBuildNewLevel(levelCount);
			levelModel.AddLevel(entity);
		}
		
	
		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}