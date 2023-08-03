using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class MainGame_Test : SavableArchitecture<MainGame_Test> {
	protected override void Init() {
		this.RegisterModel<IEntityModel>(new EntityModel());
	}

	protected override string saveFileSuffix { get; } = "test";
}
