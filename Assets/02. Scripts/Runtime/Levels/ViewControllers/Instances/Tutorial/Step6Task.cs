using System.Collections;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Controls;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {
	public struct OnClearTaskPanel {
		
	}
	public class Step6Task : PlayerTask {
		
		private float timer;
		public override string GetDescription() {
			var action = ClientInput.Singleton.FindActionInPlayerActionMap("Slide");
			string localizedKey  = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(action);
			return Localization.GetFormat("TUTORIAL_STEP6", localizedKey);
		}

		public override bool IsSatisfied() {
			if (ClientInput.Singleton.GetPlayerActions().Slide.IsPressed()) {
				timer += Time.deltaTime;
			}

			return timer >= 3;
		}

		public override void OnFinish() {
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(1f);
			this.SendEvent<OnClearTaskPanel>();
		}
	}
}