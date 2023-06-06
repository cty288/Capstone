using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class ControllerExample : MonoBehaviour, IController {
	public IArchitecture GetArchitecture() {
		return KillGame.Interface;
	}
}


