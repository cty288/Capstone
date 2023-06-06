using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using UnityEngine;

public class KillCountQuery : AbstractQuery<int> {
	protected override int OnDo() {
		return this.GetModel<IPlayerModel>().KillCount;
	}
}
