using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Levels.Sandstorm;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Instance;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Polyglot;
using Runtime.Controls;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {


	
	public class Step18Task : PlayerTask, ICanGetModel, ICanRegisterEvent {
		[ES3Serializable]
		private bool satisfied = false;

		[ES3Serializable] private bool started = false;
		private ILevelModel levelModel;
		public override void OnInit() {
			base.OnInit();
			levelModel = this.GetModel<ILevelModel>();
			levelModel.CurrentLevel.RegisterOnValueChanged(OnLevelChanged);
		}

		private void OnLevelChanged(ILevelEntity oldLevel, ILevelEntity newLevel) {
			if (newLevel is not TutorialLevelEntity) {
				satisfied = true;
			}
		}

		
		public override string GetDescription() {
			return Localization.Get("TUTORIAL_STEP18_TASK");
		}

		public override bool IsSatisfied() {
			if (!started) {
				started = true;
				this.SendEvent<OnSpawnEnemyGroup>(new OnSpawnEnemyGroup() {Index = 1});
			}
			return satisfied;
		}
		
		public override void OnFinish() {
			levelModel.CurrentLevel.UnRegisterOnValueChanged(OnLevelChanged);
		}

		

		
	}
}