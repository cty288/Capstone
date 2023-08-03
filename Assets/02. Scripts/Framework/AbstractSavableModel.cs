using System.Collections;
using System.Collections.Generic;using MikroFramework.Architecture;
using UnityEngine;

[ES3Serializable]
public abstract class AbstractSavableModel : AbstractModel {
	protected override void OnInit() {
		
	}

	public void Save(string suffix) {
		ES3.Save("Model_" + this.GetType().Name, this, $"models_{suffix}.es3");
		OnSave(suffix);
	}
	
	public virtual void OnLoad(string suffix) {
		
	}
	
	public virtual void OnSave(string suffix) {
		
	}
}
