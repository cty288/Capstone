using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using Polyglot;
using Runtime.Controls;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {
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
			
		}
	}
}