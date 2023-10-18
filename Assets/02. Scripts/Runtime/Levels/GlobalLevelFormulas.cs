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

		private static float Gety2(int rarity, int level, bool inverse = false) {
			return ((1 + (rarity - 1) / 10f) +
			        (LEVEL_EXP_UPGRADE_COFF * (level - 1)) *
			        Mathf.Pow(level, LAYER_CURVE_COFF) * (1 + (rarity - 1) / 10f));
		}
		
		public static PropertyModifier<T> GetGeneralEnemyAbilityModifier<T>(Func<int> rarityGetter, Func<int> levelGetter, bool inverse = false) {
			return (baseVal) => {
				dynamic baseValDynamic = baseVal;
				float y2 = Gety2(rarityGetter.Invoke(), levelGetter.Invoke());
				if (inverse) {
					y2 = 1 / y2;
				}
				
				if (baseVal is int || baseVal is long || baseVal is short) {
					return Mathf.RoundToInt(baseValDynamic * y2);
					
				}else if (baseVal is float || baseVal is double || baseVal is decimal) {
					return baseValDynamic * y2;

				}else if (baseVal is HealthInfo) {
					dynamic healthInfo = baseValDynamic;
					healthInfo.MaxHealth = Mathf.RoundToInt(healthInfo.MaxHealth * y2);
					healthInfo.CurrentHealth = healthInfo.MaxHealth;
					return healthInfo;
				}else if (baseVal is Vector2) {
					dynamic vector2 = baseValDynamic;
					vector2.x = Mathf.RoundToInt(vector2.x * y2);
					vector2.y = Mathf.RoundToInt(vector2.y * y2);
					return vector2;
				}else if (baseVal is Vector3) {
					dynamic vector3 = baseValDynamic;
					vector3.x = Mathf.RoundToInt(vector3.x * y2);
					vector3.y = Mathf.RoundToInt(vector3.y * y2);
					vector3.z = Mathf.RoundToInt(vector3.z * y2);
					return vector3;
				}else if (baseVal is Vector4) {
					dynamic vector4 = baseValDynamic;
					vector4.x = Mathf.RoundToInt(vector4.x * y2);
					vector4.y = Mathf.RoundToInt(vector4.y * y2);
					vector4.z = Mathf.RoundToInt(vector4.z * y2);
					vector4.w = Mathf.RoundToInt(vector4.w * y2);
					return vector4;
				}else if (baseVal is Vector2Int) {
					dynamic vector2Int = baseValDynamic;
					vector2Int.x = Mathf.RoundToInt(vector2Int.x * y2);
					vector2Int.y = Mathf.RoundToInt(vector2Int.y * y2);
					return vector2Int;
				}else if (baseVal is Vector3Int) {
					dynamic vector3Int = baseValDynamic;
					vector3Int.x = Mathf.RoundToInt(vector3Int.x * y2);
					vector3Int.y = Mathf.RoundToInt(vector3Int.y * y2);
					vector3Int.z = Mathf.RoundToInt(vector3Int.z * y2);
					return vector3Int;
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
				}else if (baseVal is Vector2) {
					dynamic vector2 = baseVal;
					vector2.x = Mathf.RoundToInt(vector2.x * Mathf.Sqrt(y2));
					vector2.y = Mathf.RoundToInt(vector2.y * Mathf.Sqrt(y2));
					return vector2;
				}else if (baseVal is Vector3) {
					dynamic vector3 = baseVal;
					vector3.x = Mathf.RoundToInt(vector3.x * Mathf.Sqrt(y2));
					vector3.y = Mathf.RoundToInt(vector3.y * Mathf.Sqrt(y2));
					vector3.z = Mathf.RoundToInt(vector3.z * Mathf.Sqrt(y2));
					return vector3;
				}else if (baseVal is Vector4) {
					dynamic vector4 = baseVal;
					vector4.x = Mathf.RoundToInt(vector4.x * Mathf.Sqrt(y2));
					vector4.y = Mathf.RoundToInt(vector4.y * Mathf.Sqrt(y2));
					vector4.z = Mathf.RoundToInt(vector4.z * Mathf.Sqrt(y2));
					vector4.w = Mathf.RoundToInt(vector4.w * Mathf.Sqrt(y2));
					return vector4;
				}else if (baseVal is Vector2Int) {
					dynamic vector2Int = baseVal;
					vector2Int.x = Mathf.RoundToInt(vector2Int.x * Mathf.Sqrt(y2));
					vector2Int.y = Mathf.RoundToInt(vector2Int.y * Mathf.Sqrt(y2));
					return vector2Int;
				}else if (baseVal is Vector3Int) {
					dynamic vector3Int = baseVal;
					vector3Int.x = Mathf.RoundToInt(vector3Int.x * Mathf.Sqrt(y2));
					vector3Int.y = Mathf.RoundToInt(vector3Int.y * Mathf.Sqrt(y2));
					vector3Int.z = Mathf.RoundToInt(vector3Int.z * Mathf.Sqrt(y2));
					return vector3Int;
				}
				
				
				throw new System.Exception(
					"Type not supported for general enemy ability modifier. Only int and float are supported.");
			};
		}
	}
}