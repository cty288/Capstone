﻿using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Properties;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Skills.Model.Properties {

	public interface ISkillCoolDown : IDictionaryProperty<int, float>, ILoadFromConfigProperty {
		//public float GetCooldownByLevel(int level);

		public float GetMaxCooldownByLevel(int level);
		
		
	}
	
	public class SkillCooldown : LoadFromConfigDictProperty<int, float>, ISkillCoolDown {
		protected override PropertyName GetPropertyName() {
			return PropertyName.skill_cooldown;
		}
		
		

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

		/*public float GetCooldownByLevel(int level) {
			return GetCooldownByLevel(level, RealValue.Value);
		}*/

		private float GetCooldownByLevel(int level, Dictionary<int, float> target) {
			if (target.TryGetValue(level, out var byLevel)) {
				return byLevel;
			}
			
			//fallback to the first value less than level
			int lastLevel = 0;
			foreach (var levelCooldown in target) {
				if (levelCooldown.Key > level) {
					break;
				}

				lastLevel = levelCooldown.Key;
			}
			
			if (target.TryGetValue(lastLevel, out var lastLevelCooldown)) {
				return lastLevelCooldown;
			}

			return -1;
		}

		public float GetMaxCooldownByLevel(int level) {
			return GetCooldownByLevel(level, RealValue.Value);
		}
	}
	
}