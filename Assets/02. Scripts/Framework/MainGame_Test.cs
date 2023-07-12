using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class MainGame_Test : Architecture<MainGame_Test> {
	protected override void Init() {
		this.RegisterModel<IEntityModel>(new EntityModel());
	}

}
