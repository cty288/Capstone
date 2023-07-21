using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public abstract class SavableArchitecture<T> : Architecture<T> where T: Architecture<T>, new()
{
	protected List<AbstractSavableModel> savableModels = new List<AbstractSavableModel>();
	protected List<AbstractSavableSystem> savableSystems = new List<AbstractSavableSystem>();

	protected const bool IsSave = true;
	
	public void RegisterModel<T>(T defaultModel) where T: class, IModel{
		if (defaultModel.GetType().IsSubclassOf(typeof(AbstractSavableModel)) && IsSave) {
			T model = ES3.Load("Model_" + defaultModel.GetType().Name, "models.es3", defaultModel as AbstractSavableModel) as T;
			
			base.RegisterModel<T>(model);
			(model as AbstractSavableModel)?.OnLoad();
			savableModels.Add(model as AbstractSavableModel);
		}
		else {
			base.RegisterModel<T>(defaultModel);
		}
	}

	public void RegisterSystem<T>(T defaultSystem) where T:class, ISystem{
		
        
		if (defaultSystem.GetType().IsSubclassOf(typeof(AbstractSavableSystem)) && IsSave) {
            
			T system = ES3.Load<AbstractSavableSystem>("System_" + defaultSystem.GetType().Name, "systems.es3", defaultSystem as AbstractSavableSystem) as T;
			base.RegisterSystem<T>(system);
			(system as AbstractSavableSystem)?.OnLoad();
			savableSystems.Add(system as AbstractSavableSystem);
		}
		else {
			base.RegisterSystem<T>(defaultSystem);
		}
       
       
	}

	public void SaveGame() {
		if (!IsSave) {
			return;
		}
		foreach (AbstractSavableModel savableModel in savableModels) {
			savableModel.Save();
		}
		foreach (AbstractSavableSystem savableSystem in savableSystems) {
			savableSystem.Save();
		}
		ES3AutoSaveMgr.Current.Save();
	}
    
	public static void ClearSave() {
		ES3.DeleteFile("models.es3");
		ES3.DeleteFile("systems.es3");
		ES3.DeleteFile("SaveFile.es3");
	}
}
