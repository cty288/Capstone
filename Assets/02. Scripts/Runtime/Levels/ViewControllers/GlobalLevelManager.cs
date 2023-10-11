using System;
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

		private ILevelModel levelModel;
		private void Awake() {
			levelModel = this.GetModel<ILevelModel>();
			levels.Shuffle();
			levelModel.CurrentLevelCount.RegisterWithInitValue(OnCurrentLevelNumChanged).UnRegisterWhenGameObjectDestroyed(gameObject);
		}

		private void OnCurrentLevelNumChanged(int arg1, int levelCount) {
			GameObject level = levels[0];
			levels.RemoveAt(0);

			GameObject spawnedLevel = Instantiate(level, Vector3.zero, Quaternion.identity);
			ILevelViewController levelViewController = spawnedLevel.GetComponent<ILevelViewController>();
			levelViewController.SetLevelNumber(levelCount);
			ILevelEntity entity = levelViewController.OnBuildNewLevel();
			levelViewController.InitWithID(entity.UUID);
			
			levelModel.CurrentLevel = entity;
			
			levelViewController.Init();
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}