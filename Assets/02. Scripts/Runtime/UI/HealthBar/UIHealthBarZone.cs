using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using UnityEngine;

public class UIHealthBarZone : AbstractMikroController<MainGame>, ISingleton {
	public static UIHealthBarZone Singleton {
		get {
			return SingletonProperty<UIHealthBarZone>.Singleton;
		}
	}

	public void OnSingletonInit() {
		
	}
	
	
	public void SpawnHealthBarObject(HealthBar healthBar) {
		healthBar.transform.SetParent(transform);
		healthBar.transform.localScale = Vector3.one;
		healthBar.transform.localPosition = Vector3.zero;
	}
}
