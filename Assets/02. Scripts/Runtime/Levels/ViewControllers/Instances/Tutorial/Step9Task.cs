using System.Collections;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Controls;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {


	public class Step9Task : PlayerTask, ICanGetModel, ICanRegisterEvent {
		private bool satisfied = false;
		public override void OnInit() {
			base.OnInit();
			this.RegisterEvent<OnCollectableResourceDestroyed>(OnCollectableResourceDestroyed);
		}

		private void OnCollectableResourceDestroyed(OnCollectableResourceDestroyed e) {
			satisfied = true;
		}

		public override string GetDescription() {
			var action = ClientInput.Singleton.FindActionInMaps("Shoot");
			string localizedKey  = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(action);
			return Localization.GetFormat("TUTORIAL_STEP9_TASK", localizedKey);
		}

		public override bool IsSatisfied() {
			return satisfied;
		}

		
		public override void OnFinish() {
			CoroutineRunner.Singleton.StartCoroutine(Finish());
		}
		
		private IEnumerator Finish() {
			yield return new WaitForSeconds(1f);
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 9
			});
			this.UnRegisterEvent<OnCollectableResourceDestroyed>(OnCollectableResourceDestroyed);
		}

		
	}
}