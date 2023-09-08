using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestPoolControl : MonoBehaviour, IController, ICanSendEvent {
	private SafeGameObjectPool pool;
	private List<GameObject> cubes = new List<GameObject>();
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
			cubes.Add(obj);
			obj.transform.position = Random.insideUnitSphere * 10;
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			if (cubes.Count > 0) {
				pool.Recycle(cubes[0]);
				cubes.RemoveAt(0);
			}
		}

		if (Input.GetKeyDown(KeyCode.E)) {
			this.SendEvent<OnKillEvent>();
		}
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
}
