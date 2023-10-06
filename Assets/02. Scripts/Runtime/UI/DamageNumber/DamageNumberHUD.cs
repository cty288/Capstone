using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using MikroFramework.Singletons;
using UnityEngine;

public class DamageNumberHUD : AbstractMikroController<MainGame>, ISingleton {
	private SafeGameObjectPool hudPool;
	private Camera mainCamera;

	[SerializeField] private float hideDistance = 50f;

	private Dictionary<DamageNumberViewController, Vector3> spawnedHuds = new Dictionary<DamageNumberViewController, Vector3>();
	private List<DamageNumberViewController> recycleList = new List<DamageNumberViewController>();
	public void OnSingletonInit() {
		
	}
	
	public static DamageNumberHUD Singleton {
		get {
			return SingletonProperty<DamageNumberHUD>.Singleton;
		}
	}

	private void Awake() {
		hudPool = GameObjectPoolManager.Singleton.CreatePoolFromAB("DamageNumber", "info", 30, 100, out _);
		mainCamera = Camera.main;
	}


	public void SpawnHUD(Vector3 worldPosition, float damage, int minSizeDamage = 5, float maxSizeDamage = 50,
		float minSize = 1.2f, float maxSize = 3) {
		if (Vector3.Distance(worldPosition, mainCamera.transform.position) > hideDistance) {
			return;
		}
		
		GameObject hud = hudPool.Allocate();

		hud.transform.SetParent(transform);
		hud.transform.position = mainCamera.WorldToScreenPoint(worldPosition);
		hud.transform.localScale = new Vector3(minSize, minSize, minSize);
		hud.transform.rotation = Quaternion.identity;
		
		DamageNumberViewController hudViewController = hud.GetComponent<DamageNumberViewController>();
		hudViewController.OnRecycledAction += OnHudRecycled;
		hudViewController.StartAnimateDamage(damage, minSizeDamage, maxSizeDamage, minSize, maxSize);
		spawnedHuds.Add(hudViewController, worldPosition);
	}

	private void OnHudRecycled(DamageNumberViewController obj) {
		obj.OnRecycledAction -= OnHudRecycled;
		spawnedHuds.Remove(obj);
	}

	private void Update() {
		foreach (var hud in spawnedHuds) {
			if (Vector3.Distance(hud.Value, mainCamera.transform.position) > hideDistance) {
				recycleList.Add(hud.Key);
			}
			else {
				hud.Key.transform.position = mainCamera.WorldToScreenPoint(hud.Value);
			}
		}
		
		foreach (var hud in recycleList) {
			hud.FadeOut();
			OnHudRecycled(hud);
		}

		recycleList.Clear();
	}
}


