using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.Magazines.GunpowerEnhancement;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.SpecialBarrel;
using Runtime.DataFramework.Entities;

namespace _02._Scripts.Runtime.BuffSystem {

	public delegate IBuff BuffBuilder(IEntity buffDealer, IEntity buffOwner, int level);
	public static class BuffPool {
		private static Dictionary<IBuff, BuffBuilder> buffPool = new Dictionary<IBuff, BuffBuilder>();
		private static Dictionary<CurrencyType, BuffBuilder> weaponBuildBuffPool = new Dictionary<CurrencyType, BuffBuilder>();


		static BuffPool() {
			RegisterGeneralBuff((DustBuff.Allocate));
			RegisterGeneralBuff(((dealer, owner, level) => BleedingBuff.Allocate(1, level, dealer, owner)));
		}
		
		public static List<BuffBuilder> FindBuffs(Predicate<IBuff> predicate) {
			List<BuffBuilder> result = new List<BuffBuilder>();
			foreach (var pair in buffPool) {
				if (predicate(pair.Key)) {
					result.Add(pair.Value);
				}
			}

			return result;
		}

		public static BuffBuilder GetWeaponBuildBuff(CurrencyType currencyType) {
			if (!weaponBuildBuffPool.ContainsKey(currencyType)) {
				return null;
			}
			return weaponBuildBuffPool[currencyType];
		}
		
		public static void RegisterGeneralBuff(BuffBuilder buffBuilder) {
			IBuff buff = buffBuilder(null, null, 1);
			buffPool.Add(buff, buffBuilder);
		}
		
		public static void RegisterWeaponBuildBuff(CurrencyType currencyType, BuffBuilder buffBuilder) {
			weaponBuildBuffPool.Add(currencyType, buffBuilder);
		}
	}
}