using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Pillars.Systems;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Polyglot;
using Runtime.Controls;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {

	
	public class Step14Task : PlayerTask, ICanGetModel, ICanRegisterEvent {
		private bool satisified = false;
		
		public override void OnInit() {
			base.OnInit();
			this.RegisterEvent<OnEquippedWeaponPartsUpdate>(OnEquippedWeaponPartsUpdate);
		}

		private void OnEquippedWeaponPartsUpdate(OnEquippedWeaponPartsUpdate e) {
			if (!string.IsNullOrEmpty(e.CurrentTopPartsUUID)) {
				satisified = true;
			}
		}
		
		public override string GetDescription() {
			var action = ClientInput.Singleton.FindActionInPlayerActionMap("Inventory");
			string localizedKey  = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(action);
			return Localization.GetFormat("TUTORIAL_STEP14_TASK", localizedKey);
		}

		public override bool IsSatisfied() {
			return satisified;
		}
		
		public override void OnFinish() {
			this.UnRegisterEvent<OnEquippedWeaponPartsUpdate>(OnEquippedWeaponPartsUpdate);
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnClearTaskPanel>();
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 13
			});
		}

		
	}
}