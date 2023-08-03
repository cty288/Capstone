using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public abstract class SavableArchitecture<T> : Architecture<T> where T: Architecture<T>, new()
{
	protected List<AbstractSavableModel> savableModels = new List<AbstractSavableModel>();
	protected List<AbstractSavableSystem> savableSystems = new List<AbstractSavableSystem>();

	protected const bool IsSave = true;
	
	protected abstract string saveFileSuffix { get; }
	
	public void RegisterModel<T>(T defaultModel) where T: class, IModel{
		if (defaultModel.GetType().IsSubclassOf(typeof(AbstractSavableModel)) && IsSave) {
			T model = ES3.Load("Model_" + defaultModel.GetType().Name, $"models_{saveFileSuffix}.es3", defaultModel as AbstractSavableModel) as T;
			
			base.RegisterModel<T>(model);
			(model as AbstractSavableModel)?.OnLoad(saveFileSuffix);
			savableModels.Add(model as AbstractSavableModel);
		}
		else {
			base.RegisterModel<T>(defaultModel);
		}
	}

	public void RegisterSystem<T>(T defaultSystem) where T:class, ISystem{
		
        
		if (defaultSystem.GetType().IsSubclassOf(typeof(AbstractSavableSystem)) && IsSave) {
            
			T system = ES3.Load<AbstractSavableSystem>("System_" + defaultSystem.GetType().Name, $"systems_{saveFileSuffix}.es3", defaultSystem as AbstractSavableSystem) as T;
			base.RegisterSystem<T>(system);
			(system as AbstractSavableSystem)?.OnLoad(saveFileSuffix);
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
			savableModel.Save(saveFileSuffix);
		}
		foreach (AbstractSavableSystem savableSystem in savableSystems) {
			savableSystem.Save(saveFileSuffix);
		}
		ES3AutoSaveMgr.Current.Save();
	}
    
	public void ClearSave() {
		ES3.DeleteFile($"models_{saveFileSuffix}.es3");
		ES3.DeleteFile($"systems_{saveFileSuffix}.es3");
		ES3.DeleteFile("SaveFile.es3");
	}
}
