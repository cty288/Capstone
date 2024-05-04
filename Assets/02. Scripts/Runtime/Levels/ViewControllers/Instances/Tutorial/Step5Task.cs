using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using Polyglot;
using Runtime.Controls;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {
	public class Step5Task : PlayerTask {
		private int counter = 0;
		
		public override string GetDescription() {
			var action = ClientInput.Singleton.FindActionInMaps("Jump");
			string localizedKey  = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(action);
			return Localization.GetFormat("TUTORIAL_STEP5", localizedKey);
		}

		public override bool IsSatisfied() {
			if (ClientInput.Singleton.GetPlayerActions().Jump.WasPerformedThisFrame()) {
				counter++;
			}

			return counter >= 2;
		}

		public override void OnFinish() {
			
		}
	}
}