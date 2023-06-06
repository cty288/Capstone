using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Tutorial._06._QueriesAndUtilities;
using UnityEngine;

public class KillGame : Architecture<KillGame> {
	protected override void Init() {
		this.RegisterModel<IPlayerModel>(new PlayerModel());
		this.RegisterSystem<IAchievementSystem>(new AchievementSystem());
		
		
		this.RegisterExtensibleUtility<StupidCalculator>(new StupidCalculator());
	}
}