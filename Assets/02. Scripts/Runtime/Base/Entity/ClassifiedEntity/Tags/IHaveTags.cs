using _02._Scripts.Runtime.Common.Properties.TagProperty;

namespace _02._Scripts.Runtime.Base.Entity.ClassifiedEntity {
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