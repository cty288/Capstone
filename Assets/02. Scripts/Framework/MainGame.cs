using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class MainGame : Architecture<MainGame> {
	protected List<AbstractSavableModel> savableModels = new List<AbstractSavableModel>();
	protected List<AbstractSavableSystem> savableSystems = new List<AbstractSavableSystem>();

	protected const bool IsSave = true;
	
	
	protected override void Init() {
		
	}
	
	
	
	protected void RegisterModel<T>() where T : class, IModel, new() {
		T model = null;
        
		if (typeof(T).IsSubclassOf(typeof(AbstractSavableModel)) && IsSave) {
			model = ES3.Load<AbstractSavableModel>("Model_" + typeof(T).Name, "models.es3", new T() as AbstractSavableModel) as T;
			this.RegisterModel<T>(model);
			(model as AbstractSavableModel)?.OnLoad();
			savableModels.Add(model as AbstractSavableModel);
		}
		else {
			model = new T();
			this.RegisterModel<T>(model);
		}
       
        
	}
    
	protected void RegisterSystem<T>() where T : class, ISystem, new() {
		T system = null;
        
		if (typeof(T).IsSubclassOf(typeof(AbstractSavableSystem)) && IsSave) {
            
			system = ES3.Load<AbstractSavableSystem>("System_" + typeof(T).Name, "systems.es3", new T() as AbstractSavableSystem) as T;
			this.RegisterSystem<T>(system);
			(system as AbstractSavableSystem)?.OnLoad();
			savableSystems.Add(system as AbstractSavableSystem);
		}
		else {
			system = new T();
			this.RegisterSystem<T>(system);
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
