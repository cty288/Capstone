using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public class KillEnemyCommand : AbstractCommand<KillEnemyCommand> {
	protected GameObject enemy;
	
	public KillEnemyCommand(GameObject enemy) {
		this.enemy = enemy;
	}
	
	public KillEnemyCommand(){}
	
	protected override void OnExecute() {
		IPlayerModel playerModel = this.GetModel<IPlayerModel>();
		playerModel.AddKillCount(1);
		GameObject.Destroy(enemy);
	}
}
