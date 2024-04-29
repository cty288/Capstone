using System.Collections;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial;
using MikroFramework.Architecture;
using MikroFramework.Utilities;
using Polyglot;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.BaseLevel {
	public struct OnTalkToNPC {
		public string NPCName;
	}
	public class NPCTalkTask : PlayerTask, ICanGetModel, ICanRegisterEvent {
		private ICurrencyModel currencyModel;

		[ES3Serializable] private string npcName;
		private bool satisfied = false;
		public override void OnInit() {
			base.OnInit();
			this.RegisterEvent<OnTalkToNPC>(OnTalkToNPC);
		}

		public NPCTalkTask(string npcName) {
			this.npcName = npcName;
		}

		private void OnTalkToNPC(OnTalkToNPC e) {
			if (e.NPCName == npcName) {
				satisfied = true;
			}
		}


		public override string GetDescription() {
			return Localization.Get($"BASE_TUTORIAL_TASK_{npcName}");
		}

		public override bool IsSatisfied() {
			return satisfied;
		}
		
		public override void OnFinish() {
			this.UnRegisterEvent<OnTalkToNPC>(OnTalkToNPC);
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnClearTask>(new OnClearTask() {
				Task = this
			});
		}
	}
}