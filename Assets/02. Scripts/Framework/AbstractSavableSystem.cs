using MikroFramework.Architecture;

namespace Runtime.Framework {
	[ES3Serializable]
	public abstract class AbstractSavableSystem : AbstractSystem{
		protected override void OnInit() {
		
		}

		public void Save(string suffix) {
			ES3.Save("System_" + this.GetType().Name, this, $"systems_{suffix}.es3");
			OnSave(suffix);
		}
	
		public virtual void OnLoad(string suffix) {
		
		}
	
		public virtual void OnSave(string suffix) {
		
		}
	}
}
