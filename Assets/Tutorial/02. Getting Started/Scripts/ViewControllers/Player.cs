using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class Player : AbstractMikroController<KillGame> {
	private IPlayerModel playerModel;

	private void Awake() {
		playerModel = this.GetModel<IPlayerModel>();
	}

	private void Update() {
		if (Input.GetMouseButtonDown(0)) {
			//check if there are any gameobjects with tag "Enemy" surrounding the player
			Collider[] colliders = Physics.OverlapSphere(transform.position, 3f);
			
			foreach (var collider in colliders) {
				GameObject go = collider.gameObject;
				if (go.CompareTag("Enemy")) {
					//method 1: access the model directly
					/*playerModel.AddKillCount(1);
					Destroy(go);*/
					
					//method 2: use command
					this.SendCommand<KillEnemyCommand>(new KillEnemyCommand(go));
				}
			}
		}
	}
}
