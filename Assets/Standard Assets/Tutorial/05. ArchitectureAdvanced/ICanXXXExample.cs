using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class CanSendEventClass : ICanSendEvent {
	public IArchitecture GetArchitecture() {
		return KillGame.Interface;
	}
}


public class CanGetModelClass : ICanGetModel {
	public IArchitecture GetArchitecture() {
		return KillGame.Interface;
	}
}


public class CanRegisterEventClass : ICanRegisterEvent {
	public IArchitecture GetArchitecture() {
		return KillGame.Interface;
	}
}

public class CanSendCommandClass : ICanSendCommand {
	public IArchitecture GetArchitecture() {
		return KillGame.Interface;
	}
}