using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Utilities;
using UnityEngine;

public class PooledCubeTest : AbstractMikroController<MainGame>
{
	private void Awake() {
		this.GetComponent<PoolableGameObject>().RegisterOnAllocateEvent(OnAllocate);
		
	}

	private void OnAllocate() {
		this.RegisterEvent<OnKillEvent>(OnKillEvent).
			UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
	}

	private void OnKillEvent(OnKillEvent obj) {
		Debug.Log("Kill event received");
	}
}
