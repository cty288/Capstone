using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Entities.Enemies;
using MikroFramework.Architecture;
using UnityEngine;

public class MainGame_Test : SavableArchitecture<MainGame_Test> {
	protected override void Init() {
		this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
		this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
	}

	protected override string saveFileSuffix { get; } = "test";
}
