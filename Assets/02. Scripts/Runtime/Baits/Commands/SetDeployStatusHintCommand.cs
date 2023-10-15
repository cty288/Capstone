using MikroFramework.Architecture;
using MikroFramework.Pool;
using Polyglot;

namespace _02._Scripts.Runtime.Baits.Commands {
	public struct OnSetDeployStatusHint {
		public string hint;
	}
	public class SetDeployStatusHintCommand : AbstractCommand<SetDeployStatusHintCommand> {
		private string localizationKey;
		private object[] args;
		protected override void OnExecute() {
			string targetStr = "";
			if (!string.IsNullOrEmpty(localizationKey)) {
				targetStr = Localization.GetFormat(localizationKey, args);
			}

			this.SendEvent<OnSetDeployStatusHint>(new OnSetDeployStatusHint() {hint = targetStr});
		}
		
		public SetDeployStatusHintCommand() {
			
		}
		
		public static SetDeployStatusHintCommand Allocate(string localizationKey, params object[] args) {
			SetDeployStatusHintCommand command = SafeObjectPool<SetDeployStatusHintCommand>.Singleton.Allocate();
			command.localizationKey = localizationKey;
			command.args = args;
			return command;
		}
		
		
	}
}