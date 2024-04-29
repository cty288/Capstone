using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using UnityEngine;

public class ResourceBreakDetector : MonoBehaviour , ICanSendEvent{
	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
}
