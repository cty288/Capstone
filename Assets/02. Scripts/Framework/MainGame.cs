using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Common.Entities.Enemies;
using MikroFramework.Architecture;
using UnityEngine;

public class MainGame : SavableArchitecture<MainGame> {
	protected override void Init() {
		this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
		this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
	}

	protected override string saveFileSuffix { get; } = "main";
}
