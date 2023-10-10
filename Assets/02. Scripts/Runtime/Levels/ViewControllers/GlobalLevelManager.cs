using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public class GlobalLevelManager : MonoMikroSingleton<GlobalLevelManager>, IController {
		[SerializeField] protected List<GameObject> levels = new List<GameObject>();

		private ILevelModel levelModel;
		private void Awake() {
			
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}