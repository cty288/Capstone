using System;
using MikroFramework.Singletons;

namespace Runtime.Utilities {
	public class CoroutineRunner : MonoMikroSingleton<CoroutineRunner> {
		private Action OnUpdate;
		private void Update() {
			OnUpdate?.Invoke();
		}
		
		public void RegisterUpdate(Action action) {
			OnUpdate += action;
		}
		
		public void UnregisterUpdate(Action action) {
			OnUpdate -= action;
		}
	}
}
