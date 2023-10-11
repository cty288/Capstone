using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Commands;
using Framework;
using MikroFramework.Architecture;
using UnityEngine;

public class NextLevelDoor : AbstractMikroController<MainGame> {
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			this.SendCommand<NextLevelCommand>(NextLevelCommand.Allocate());
		}
	}
}
