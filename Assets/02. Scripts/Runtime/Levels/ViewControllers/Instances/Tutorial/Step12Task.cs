using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
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


	
	public class Step12Task : PlayerTask, ICanGetModel, ICanRegisterEvent {
		[ES3Serializable]
		private int killCount;
		[ES3Serializable]
		private int targetKillCount = 5;

		public Step12Task(int targetKillCount) {
			this.targetKillCount = targetKillCount;
		}
		public override void OnInit() {
			base.OnInit();
			this.RegisterEvent<OnEnemyDie>(OnEnemyDie);
		}

		private void OnEnemyDie(OnEnemyDie obj) {
			killCount++;
		}


		public override string GetDescription() {
			return Localization.Get("TUTORIAL_STEP12_TASK");
		}

		public override bool IsSatisfied() {
			return killCount >= targetKillCount;
		}
		
		public override void OnFinish() {
			this.UnRegisterEvent<OnEnemyDie>(OnEnemyDie);
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnClearTaskPanel>();
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 12
			});
		}

		
	}
}