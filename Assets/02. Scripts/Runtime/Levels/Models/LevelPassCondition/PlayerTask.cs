using Framework;
using MikroFramework.Architecture;

namespace _02._Scripts.Runtime.Levels.Models.LevelPassCondition {
	public abstract class PlayerTask : ICanSendEvent {
		public abstract string GetDescription();
		public abstract bool IsSatisfied();

		public abstract void OnFinish();

		public virtual void OnInit() {
			
		}
		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}