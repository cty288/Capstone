using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Weapons.ViewControllers.Base;

namespace Runtime.Weapons.Commands {
	
	public class ScopeCommand : AbstractCommand<ScopeCommand> {
		private bool _isScopedIn;
		protected override void OnExecute() {
			this.SendEvent<OnScopeUsedEvent>(new OnScopeUsedEvent()
			{
				isScopedIn = _isScopedIn
			});
		}
		
		
		public ScopeCommand() {
			
		}
		
		public static ScopeCommand Allocate(bool isScopedIn) {
			ScopeCommand command = SafeObjectPool<ScopeCommand>.Singleton.Allocate();
			command._isScopedIn = isScopedIn;
			return command;
		}
	}
}