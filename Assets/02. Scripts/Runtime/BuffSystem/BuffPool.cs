using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Mineral;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Time;
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
			RegisterGeneralBuff((MotivatedBuff.Allocate));
			RegisterGeneralBuff((dealer, owner, level) => HackedBuff.Allocate(dealer, owner, 10, 2, 1));
			RegisterGeneralBuff(((dealer, owner, level) => MalfunctionBuff.Allocate(dealer, owner, 5)));
			RegisterGeneralBuff(((dealer, owner, level) => PowerlessBuff.Allocate(dealer, owner, level, 10)));


			RegisterWeaponBuildBuff(CurrencyType.Time, TimeBuff.Allocate);
			RegisterWeaponBuildBuff(CurrencyType.Combat, CombatBuff.Allocate);
			RegisterWeaponBuildBuff(CurrencyType.Plant, PlantBuff.Allocate);
			RegisterWeaponBuildBuff(CurrencyType.Mineral, MineralBuff.Allocate);
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
		
		public static List<IBuff> GetTemplateBuffs(Predicate<IBuff> predicate) {
			List<IBuff> result = new List<IBuff>();
			foreach (var pair in buffPool) {
				if (predicate(pair.Key)) {
					result.Add(pair.Key);
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