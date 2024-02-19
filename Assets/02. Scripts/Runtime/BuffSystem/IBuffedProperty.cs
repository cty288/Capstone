using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Utilities;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.BuffSystem {
	public interface IBuffedProperty : IPropertyBase {
		ReferenceCounter IsBuffedRC { get; set; }

		public bool GetIsBuffed() {
			return IsBuffedRC.Count > 0;
		}
		
		HashSet<BuffTag> BuffTags { get; }
	}
	public interface IBuffedProperty<T> : IProperty<T>, IBuffedProperty {
		
	}
	

	public enum BuffTag {
		TestBuff1 = 1,
		TestBuff2,
		TestBuff3,
		Weapon_AttackSpeed,
		Weapon_BaseDamage,
		Weapon_AmmoSize,
		Weapon_HipFireRecoil,
		Weapon_ScopeRecoil,
	}
}