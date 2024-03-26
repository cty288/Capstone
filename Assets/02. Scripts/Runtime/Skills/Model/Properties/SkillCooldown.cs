﻿using System.Collections.Generic;
using System.Linq;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Properties;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Skills.Model.Properties {
	public interface ILeveledProperty<T> : IDictionaryProperty<int, T>, ILoadFromConfigProperty {
		public T GetByLevel(int level);
		public int GetMaxLevel();
	}

	public interface ISkillCoolDown : ILeveledProperty<float>, ILoadFromConfigProperty {
		
	}
	
	public abstract class LeveledProperty<T> : LoadFromConfigDictProperty<int, T>, ILeveledProperty<T> {
		protected T GetByLevel(int level, Dictionary<int, T> target) {
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

			return default;
		}
		
		
		public T GetByLevel(int level) {
			return GetByLevel(level, RealValue.Value);
		}
		
		public int GetMaxLevel() {
			return RealValue.Value.Keys.Max();
		}
	}
	
	public class SkillCooldown : LeveledProperty<float>, ISkillCoolDown {
		protected override PropertyName GetPropertyName() {
			return PropertyName.skill_cooldown;
		}
		
		

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

		/*public float GetCooldownByLevel(int level) {
			return GetCooldownByLevel(level, RealValue.Value);
		}*/
		
	}
	
}