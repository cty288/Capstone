using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Pool;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestPoolControl : MonoBehaviour {
	private SafeGameObjectPool pool;
	private void Awake() { 
		pool = GameObjectPoolManager.Singleton.CreatePoolFromAB("PooledCube", "others", 10, 50,
			out GameObject prefab);
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.S)) {
			((MainGame) MainGame.Interface).SaveGame();
		}


		if (Input.GetKeyDown(KeyCode.A)) {
			GameObject obj = pool.Allocate();
			obj.transform.position = Random.insideUnitSphere * 10;
		}
	}
}
