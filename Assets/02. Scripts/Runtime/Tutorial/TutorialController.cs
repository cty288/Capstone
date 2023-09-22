using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.Inventory.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialController : AbstractMikroController<MainGame>
{
	private void Update() {
		if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) {
			this.GetSystem<IInventorySystem>().ClearSlots();
			this.Delay(0.2f, () => {
				SceneManager.LoadScene("MainGame");
			});
		
		}
	}
}
