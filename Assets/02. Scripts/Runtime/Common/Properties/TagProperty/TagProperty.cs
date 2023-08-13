using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Entity.ClassifiedEntity;
using _02._Scripts.Runtime.Base.Property;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _02._Scripts.Runtime.Common.Properties.TagProperty {
	public interface ITagProperty : IProperty<Dictionary<TagName,int>>, ILoadFromConfigProperty {
		/// <summary>
		/// Get all tags of this entity
		/// </summary>
		/// <returns></returns>
		public Tag[] GetTags();
		
		/// <summary>
		/// Check if this entity has a tag
		/// </summary>
		/// <param name="tagName"></param>
		/// <returns></returns>
		public bool HasTag(TagName tagName);
		
		/// <summary>
		/// Check if this entity has a tag and get its level if it has
		/// </summary>
		/// <param name="tagName"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public bool HasTag(TagName tagName, out int level);
		
		/// <summary>
		/// Get the level of a tag. If the entity doesn't have this tag, an exception will be thrown
		/// </summary>
		/// <param name="tagName"></param>
		/// <returns></returns>
		
		public int GetTagLevel(TagName tagName);
		
		/// <summary>
		/// Check if this entity has a tag with a level greater than or equal to the given level. If the entity doesn't have this tag, it will always return false
		/// </summary>
		/// <param name="tagName"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		
		public bool HasTagOverLevel(TagName tagName, int level);
	}
	
	public class TagProperty : LoadFromConfigDictProperty<TagName, int>, ITagProperty {
		
		public TagProperty() : base() {
		}
		
		public TagProperty(Dictionary<TagName, int> tags) : base() {
			BaseValue = tags;
		}
		
		protected override IPropertyDependencyModifier<Dictionary<TagName, int>> GetDefautModifier() {
			return null;
		}

		protected override PropertyName GetPropertyName() {
			return PropertyName.tags;
		}

		public override PropertyNameInfo[] GetDefaultDependentProperties() {
			return null;
		}

		public override Dictionary<TagName, int> OnSetBaseValueFromConfig(dynamic value) {
			return value.ToObject<Dictionary<TagName,int>>();
		}

		public Tag[] GetTags() {
			Tag[] tags = new Tag[RealValues.Value.Count];
			int i = 0;
			foreach (KeyValuePair<TagName,int> keyValuePair in RealValues.Value) {
				tags[i] = new Tag(keyValuePair.Key, keyValuePair.Value);
				i++;
			}
			return tags;
		}

		public bool HasTag(TagName tagName) {
			return RealValues.Value.ContainsKey(tagName);
		}

		public bool HasTag(TagName tagName, out int level) {
			return RealValues.Value.TryGetValue(tagName, out level);
		}

		public int GetTagLevel(TagName tagName) {
			if (HasTag(tagName, out int level)) {
				return level;
			}
			else {
				throw new KeyNotFoundException($"Tag {tagName} not found");
			}
		}

		public bool HasTagOverLevel(TagName tagName, int level) {
			if (HasTag(tagName, out int lv)) {
				return lv >= level;
			}
			else {
				return false;
			}
		}
	}
}