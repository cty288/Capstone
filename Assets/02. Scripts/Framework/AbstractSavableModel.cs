using MikroFramework.Architecture;

namespace Framework {
	public interface ISavableModel : IModel {
		void OnLoad(string suffix);
		void OnSave(string suffix);
		
		bool IsFirstTimeCreated { get; set; }
	}
	
	
	[ES3Serializable]
	public abstract class AbstractSavableModel : AbstractModel, ISavableModel {
		protected override void OnInit() {
		
		}

		public void Save(string suffix) {
			IsFirstTimeCreated = false;
			ES3.Save("Model_" + this.GetType().Name, this, $"models_{suffix}.es3");
			OnSave(suffix);
		}
	
		public virtual void OnLoad(string suffix) {
		
		}
	
		public virtual void OnSave(string suffix) {
		
		}

		[field: ES3Serializable] 
		public bool IsFirstTimeCreated { get; set; } = true;
	}
}
