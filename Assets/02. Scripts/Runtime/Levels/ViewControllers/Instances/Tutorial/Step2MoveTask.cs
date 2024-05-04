using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using Polyglot;
using Runtime.Controls;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {
	public class Step2MoveTask : PlayerTask {

		[ES3Serializable] private float minTime = 3f;
		private float timer = 0;
		public override string GetDescription() {
			var action = ClientInput.Singleton.FindActionInMaps("SprintHold");
			string localizedKey  = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(action);
			return Localization.GetFormat("TUTORIAL_STEP2", localizedKey);
		}

		public override bool IsSatisfied() {
			if (ClientInput.Singleton.GetPlayerActions().Move.IsPressed()) {
				timer += Time.deltaTime;
			}
			
			if (timer >= minTime) {
				return true;
			}

			return false;
		}

		public override void OnFinish() {
			
		}
	}
}