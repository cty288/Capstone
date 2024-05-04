using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
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


	
	public class Step16Task : PlayerTask, ICanGetModel, ICanRegisterEvent {

		private bool satisfied = false;
		public override void OnInit() {
			base.OnInit();
			this.RegisterEvent<OnSkillUsed>(OnSkillUsed);
		}

		private void OnSkillUsed(OnSkillUsed e) {
			if (e.skillEntity is MedicalNeedleSkill) {
				satisfied = true;
			}
		}


		public override string GetDescription() {
			return Localization.Get("TUTORIAL_STEP16_TASK");
		}

		public override bool IsSatisfied() {
			return satisfied;
		}
		
		public override void OnFinish() {
			this.UnRegisterEvent<OnSkillUsed>(OnSkillUsed);
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 16
			});
		}

		
	}
}