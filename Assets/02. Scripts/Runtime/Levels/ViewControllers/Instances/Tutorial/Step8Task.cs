using System.Collections;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Controls;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {

	public struct OnTutorialTaskFinish {
		public int TaskID;
	}
	public class Step8Task : PlayerTask, ICanGetModel {
		
		private float timer;
		
		
		public override string GetDescription() {
			var action = ClientInput.Singleton.FindActionInPlayerActionMap("Inventory");
			string localizedKey  = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(action);
			return Localization.GetFormat("TUTORIAL_STEP8_TASK", localizedKey);
		}

		public override bool IsSatisfied() {
			IInventoryModel inventoryModel = this.GetModel<IInventoryModel>();
			return inventoryModel.GetSelectedHotBarSlot(HotBarCategory.Right).GetQuantity() > 0;
		}

		public override void OnFinish() {
			CoroutineRunner.Singleton.StartCoroutine(Finish());
		}
		
		private IEnumerator Finish() {
			yield return new WaitForSeconds(1f);
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 8
			});
		}

		
	}
}