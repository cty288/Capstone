using System;
using System.Collections.Generic;
using MikroFramework.Singletons;
using UnityEngine;

namespace _02._Scripts.Runtime.Rewards {
	public class RewardDisperser : MikroSingleton<RewardDisperser> {
		private RewardDisperser() {
			
		}
		
		/// <summary>
		/// Will disperse the rewards given in the list of RewardBatches.
		/// This is an async method, because some rewards may need UI selection by the player.
		/// The callback will be called when all rewards are dispersed, including all spawned GameObjects
		/// </summary>
		/// <param name="rewardBatches"></param>
		/// <param name="onRewardsDispersed"></param>
		public void DisperseRewards(List<RewardBatch> rewardBatches, Action<List<GameObject>> onRewardsDispersed) {
			
		}
	}
}