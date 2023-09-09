using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.Utilities;
using UnityEngine;

public class PooledCubeTest : DefaultPoolableGameObjectSaved
{
	protected override void Awake() {
		base.Awake();
		Debug.Log("Awake");
	}
	
	

	/*private void Start() {
		Debug.Log("Start: " + this.GetComponent<PoolableGameObject>().Pool);
	}*/

	public override void OnStartOrAllocate() {
		base.OnStartOrAllocate();
		this.RegisterEvent<OnKillEvent>(OnKillEvent).
			UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		
		Debug.Log("StartOrAllocate: " + this.GetComponent<PoolableGameObject>().Pool);
	}
	

	private void OnKillEvent(OnKillEvent obj) {
		Debug.Log("Kill event received");
	}
}
