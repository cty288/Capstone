﻿using Runtime.DataFramework.Properties;
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
		
		
		public static PropertyModifier<dynamic> GetGeneralEnemyAbilityModifier(int rarity, int level){
			return (baseVal) => {
				if (baseVal is int || baseVal is long || baseVal is short) {
					return Mathf.RoundToInt(baseVal * ((1 + (rarity - 1) / 10f) +
					                                   (LEVEL_EXP_UPGRADE_COFF * (level - 1)) *
					                                   Mathf.Pow(level, LAYER_CURVE_COFF) * (1 + (rarity - 1) / 10f)));
					
				}else if (baseVal is float || baseVal is double || baseVal is decimal) {
					return baseVal * ((1 + (rarity - 1) / 10f) +
					                  (LEVEL_EXP_UPGRADE_COFF * (level - 1)) *
					                  Mathf.Pow(level, LAYER_CURVE_COFF) * (1 + (rarity - 1) / 10f));

				}else if (baseVal is HealthInfo) {
					HealthInfo healthInfo = baseVal;
					healthInfo.MaxHealth = Mathf.RoundToInt(healthInfo.MaxHealth * ((1 + (rarity - 1) / 10f) +
						(LEVEL_EXP_UPGRADE_COFF * (level - 1)) *
						Mathf.Pow(level, LAYER_CURVE_COFF) * (1 + (rarity - 1) / 10f)));
					
					healthInfo.CurrentHealth = healthInfo.MaxHealth;
					return healthInfo;
				}
				throw new System.Exception(
					"Type not supported for general enemy ability modifier. Only int and float are supported.");
			};
		}
	}
}