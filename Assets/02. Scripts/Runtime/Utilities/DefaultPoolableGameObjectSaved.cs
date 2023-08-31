using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using UnityEngine;

public class DefaultPoolableGameObjectSaved : DefaultPoolableGameObject, IController {
	private ES3AutoSave es3AutoSave;
	private void Awake() {
		es3AutoSave = GetComponent<ES3AutoSave>();
		this.RegisterEvent<OnBeforeGameSave>(OnBeforeSaveGame).UnRegisterWhenGameObjectDestroyed(gameObject);
	}

	private void OnBeforeSaveGame(OnBeforeGameSave e) {
		es3AutoSave.enabled = !IsRecycled;
		Debug.Log($"save game! enabled: {es3AutoSave.enabled}. Recycled: {IsRecycled}");
	}

	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
}
