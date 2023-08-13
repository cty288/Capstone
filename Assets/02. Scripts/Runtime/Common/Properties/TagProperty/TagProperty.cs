using System.Collections.Generic;
using _02._Scripts.Runtime.Base.Entity.ClassifiedEntity;
using _02._Scripts.Runtime.Base.Property;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _02._Scripts.Runtime.Common.Properties.TagProperty {
	public interface ITagProperty : IProperty<Dictionary<TagName,int>>, ILoadFromConfigProperty {
		public Tag[] GetTags();
		
		public bool HasTag(TagName tagName);
		
		public bool HasTag(TagName tagName, out int level);
		
		public int GetTagLevel(TagName tagName);
		
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
			if (GetTagLevel(tagName) >= level) {
				return true;
			}
			else {
				return false;
			}
		}
	}
}