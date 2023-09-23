using Runtime.DataFramework.Properties.TagProperty;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Tags {
	public struct Tag {
		public TagName Name;
		public int Level;
		
		public Tag(TagName name, int level) {
			Name = name;
			Level = level;
		}
	}
	public interface IHaveTags : IEntity {
		public ITagProperty GetTagProperty();
	}
}