using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Properties;
using Runtime.Utilities;

namespace _02._Scripts.Runtime.Skills.Model.Properties {
	public interface ISkillLevelProperty<T> : IDictionaryProperty<int, T>, ILoadFromConfigProperty {
		public T GetByLevel(int level);
	}

	public interface ISkillCoolDown : ISkillLevelProperty<float>, ILoadFromConfigProperty {
		
	}
	
	public abstract class SkillLevelProperty<T> : LoadFromConfigDictProperty<int, T>, ISkillLevelProperty<T> {
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
	}
	
	public class SkillCooldown : SkillLevelProperty<float>, ISkillCoolDown {
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