using System.Collections.Generic;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;

namespace Runtime.Inventory.Commands {
	public struct OnOpenSkillUpgradePanel : UIMsg {
		public ISkillEntity skillEntity;
	}
	public class OpenSkillUpgradePanelCommand : AbstractCommand<OpenSkillUpgradePanelCommand> {
		protected ISkillEntity skillEntity;

		public OpenSkillUpgradePanelCommand() {
			
		}
		protected override void OnExecute() {
			this.SendEvent<OnOpenSkillUpgradePanel>(new OnOpenSkillUpgradePanel() {
				skillEntity = skillEntity
			});
		}
		
		public static OpenSkillUpgradePanelCommand Allocate(ISkillEntity skill) {
			OpenSkillUpgradePanelCommand command = SafeObjectPool<OpenSkillUpgradePanelCommand>.Singleton.Allocate();
			command.skillEntity = skill;
			return command;
		}
	}
}