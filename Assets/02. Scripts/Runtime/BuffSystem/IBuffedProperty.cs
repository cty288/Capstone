using System.Collections.Generic;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.BuffSystem {
	public interface IBuffedProperty : IPropertyBase {
		bool IsBuffed { get; set; }
		HashSet<BuffTag> BuffTags { get; }
	}
	public interface IBuffedProperty<T> : IProperty<T>, IBuffedProperty {
		
	}
	
	
	public enum BuffTag {
		TestBuff1 = 1,
		TestBuff2,
		TestBuff3,
		Health
	}
}