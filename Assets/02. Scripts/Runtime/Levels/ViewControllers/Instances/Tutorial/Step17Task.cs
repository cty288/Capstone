using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Levels.Sandstorm;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Instance;
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


	
	public class Step17Task : PlayerTask, ICanGetModel, ICanRegisterEvent {

		private bool satisfied = false;
		public override void OnInit() {
			base.OnInit();
			this.RegisterEvent<OnSkillUpgrade>(OnSkillUpgrade);
		}

		private void OnSkillUpgrade(OnSkillUpgrade e) {
			if (e.SkillEntity is MedicalNeedleSkill) {
				satisfied = true;
			}
		}


		public override string GetDescription() {
			return Localization.Get("TUTORIAL_STEP17_TASK");
		}

		public override bool IsSatisfied() {
			return satisfied;
		}
		
		public override void OnFinish() {
			this.UnRegisterEvent<OnSkillUpgrade>(OnSkillUpgrade);
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnClearTaskPanel>();
			
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnSandStormWarning>(new OnSandStormWarning() {
				RemainingMinutes = 999,
				IsTutorialWarning = true
			});
			yield return new WaitForSeconds(5);
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 17
			});
		}

		
	}
}