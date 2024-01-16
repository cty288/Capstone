using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using AYellowpaper.SerializedCollections;
using Runtime.Spawning;
using UnityEngine;

namespace _02._Scripts.Runtime.Rewards {
	public enum RewardType {
		Resource,
		WeaponParts_ChooseOne
	}

	[Serializable]
	public struct RewardBatch {
		public RewardType RewardType;
		public int Level;
		public Vector2Int AmountRange;
	}

	[Serializable]
	public struct PillarRewardsInfo {
		[SerializeField] [SerializedDictionary("Pillar Level", "Rewards")]
		public SerializedDictionary<int, List<RewardBatch>> RewardsInfo;
		
		public List<RewardBatch> GetRewards(int pillarLevel) {
			if (RewardsInfo.TryGetValue(pillarLevel, out var rewards)) {
				return rewards;
			}
			
			//find the closest pillar level that is smaller than the given pillar level
			int closestPillarLevel = 0;
			foreach (var pair in RewardsInfo) {
				if (pair.Key > pillarLevel) {
					break;
				}

				closestPillarLevel = pair.Key;
			}


			return RewardsInfo[closestPillarLevel];
		}
	}
}