using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using Polyglot;
using Runtime.Controls;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {
	public class Step3Task : PlayerTask {

		
		public override string GetDescription() {
			var action = ClientInput.Singleton.FindActionInPlayerActionMap("Jump");
			string localizedKey  = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(action);
			return Localization.GetFormat("TUTORIAL_STEP3", localizedKey);
		}

		public override bool IsSatisfied() {
			if (ClientInput.Singleton.GetPlayerActions().Jump.WasPerformedThisFrame()) {
				return true;
			}
			
			return false;
		}

		public override void OnFinish() {
			
		}
	}
}