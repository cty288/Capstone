using System;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Properties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels {
	public static class GlobalLevelFormulas {

		public static float LAYER_CURVE_COFF = 0f;
		public static float LEVEL_EXP_UPGRADE_COFF = 0f;
		
		static GlobalLevelFormulas() {
			LAYER_CURVE_COFF =
				float.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("LAYER_CURVE_COFF", "Value1"));

			LEVEL_EXP_UPGRADE_COFF =
				float.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("LEVEL_EXP_UPGRADE_COFF", "Value1"));
			
			
		}

		private static float Gety2(int rarity, int level) {
			return ((1 + (rarity - 1) / 10f) +
			        (LEVEL_EXP_UPGRADE_COFF * (level - 1)) *
			        Mathf.Pow(level, LAYER_CURVE_COFF) * (1 + (rarity - 1) / 10f));
		}
		
		public static PropertyModifier<T> GetGeneralEnemyAbilityModifier<T>(Func<int> rarityGetter, Func<int> levelGetter) {
			return (baseVal) => {
				dynamic baseValDynamic = baseVal;
				float y2 = Gety2(rarityGetter.Invoke(), levelGetter.Invoke());
				if (baseVal is int || baseVal is long || baseVal is short) {
					return Mathf.RoundToInt(baseValDynamic * y2);
					
				}else if (baseVal is float || baseVal is double || baseVal is decimal) {
					return baseValDynamic * y2;

				}else if (baseVal is HealthInfo) {
					dynamic healthInfo = baseValDynamic;
					healthInfo.MaxHealth = Mathf.RoundToInt(healthInfo.MaxHealth * y2);
					healthInfo.CurrentHealth = healthInfo.MaxHealth;
					return healthInfo;
				}
				throw new System.Exception(
					"Type not supported for general enemy ability modifier. Only int and float are supported.");
			};
		}

		public static PropertyModifier<T> GetSpawnCostModifier<T>(Func<int> rarityGetter, Func<int> levelGetter) {
			return (baseVal) => {
				dynamic baseValDynamic = baseVal;
				float y2 = Gety2(rarityGetter.Invoke(), levelGetter.Invoke());
				if (baseVal is int || baseVal is long || baseVal is short) {
					return Mathf.RoundToInt(baseValDynamic * Mathf.Sqrt(y2));
					
				}else if (baseVal is float || baseVal is double || baseVal is decimal) {
					return baseValDynamic * Mathf.Sqrt(y2);

				}else if (baseVal is HealthInfo) {
					dynamic healthInfo = baseVal;
					healthInfo.MaxHealth = Mathf.RoundToInt(healthInfo.MaxHealth * Mathf.Sqrt(y2));
					healthInfo.CurrentHealth = healthInfo.MaxHealth;
					return healthInfo;
				}
				throw new System.Exception(
					"Type not supported for general enemy ability modifier. Only int and float are supported.");
			};
		}
	}
}