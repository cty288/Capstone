using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Tutorial._06._QueriesAndUtilities;
using UnityEngine;

//before doc 07, this should inherit Architecture<KillGame>
public class KillGame : SavableArchitecture<KillGame> { 
	protected override void Init() {
		this.RegisterModel<IPlayerModel>(new PlayerModel());
		this.RegisterSystem<IAchievementSystem>(new AchievementSystem());
		
		
		RegisterExtensibleUtility<StupidCalculator>(new StupidCalculator());
		
	}
}