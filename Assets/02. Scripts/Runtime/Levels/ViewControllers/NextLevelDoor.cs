using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Commands;
using Framework;
using MikroFramework.Architecture;
using Runtime.UI;
using UnityEngine;

public class NextLevelDoor : AbstractMikroController<MainGame> {
	[SerializeField]
	private bool goToNextLevelByDefault = false;
	private void OnTriggerEnter(Collider other) {
		
		if (other.gameObject.CompareTag("Player")) {
			if (goToNextLevelByDefault) {
				this.SendCommand<NextLevelCommand>(NextLevelCommand.Allocate());
			}
			else {
				MainUI.Singleton.OpenOrGetClose<ExitDoorUI>(MainUI.Singleton, null);
			}
		}
	}
}
