using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Polyglot;
using Runtime.Controls;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {

	public struct OnSpawnEnemyGroup {
		public int Index;
	}
	
	public class Step11Task : PlayerTask, ICanGetModel, ICanRegisterEvent {
		private ICurrencyModel currencyModel;
		
		public override void OnInit() {
			base.OnInit();
			currencyModel = this.GetModel<ICurrencyModel>();
		}

		

		public override string GetDescription() {
			int mineralCount = currencyModel.GetCurrencyAmountProperty(CurrencyType.Mineral);
			int gasCount = currencyModel.GetCurrencyAmountProperty(CurrencyType.Plant);
			return Localization.GetFormat("TUTORIAL_STEP11_TASK", mineralCount, gasCount);
		}

		public override bool IsSatisfied() {
			int mineralCount = currencyModel.GetCurrencyAmountProperty(CurrencyType.Mineral);
			int gasCount = currencyModel.GetCurrencyAmountProperty(CurrencyType.Plant);
			return mineralCount >= 10 && gasCount >= 10;
		}
		
		public override void OnFinish() {
			this.SendEvent<OnSpawnEnemyGroup>(new OnSpawnEnemyGroup() {Index = 0});
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnClearTaskPanel>();
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 11
			});
		}

		
	}
}