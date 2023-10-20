using _02._Scripts.Runtime.Levels.Models;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Utilities;

namespace _02._Scripts.Runtime.Levels.Systems {
	public class LevelSystemExitEventHandler : ICanRegisterEvent {
		private ILevelEntity levelEntity;
		
		public void Init() {
			
		}
		
		public void SetLevelEntity(ILevelEntity levelEntity) {
			this.levelEntity = levelEntity;
		}



		public void OnOneSecondPassed() {
			
		}
		
		
		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
}