using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class MainGame : SavableArchitecture<MainGame> {
	protected override void Init() {
		this.RegisterModel<IEntityModel>(new EntityModel());
	}

}
