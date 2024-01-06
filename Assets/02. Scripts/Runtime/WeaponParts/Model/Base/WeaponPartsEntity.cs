using System;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.CustomProperties;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.WeaponParts.Model.Base {

	public enum WeaponPartType {
		Barrel,
		Magazine,
		Attachment,
	}
	
	public interface IWeaponPartsEntity : IResourceEntity, IHaveCustomProperties, IHaveTags {
		public WeaponPartType WeaponPartType { get; }
		
		public IWeaponPartsBuff OnGetBuff(IWeaponEntity weaponEntity);
		
		public Type BuffType { get;}
	}
	
	public abstract class WeaponPartsEntity<T, TBuffType> : ResourceEntity<T>, IWeaponPartsEntity
		where T : WeaponPartsEntity<T, TBuffType>, new()
		where TBuffType : WeaponPartsBuff<T, TBuffType>, new() {
		
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.WeaponPartsConfigTable;
		}
		
		protected override string OnGetDisplayNameBeforeFirstPicked(string originalDisplayName) {
			return originalDisplayName;
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return $"<b><color=red>{Localization.Get($"NAME_{WeaponPartType.ToString()}")}</color></b>\n" +
			       OnGetWeaponPartDescription(defaultLocalizationKey);
		}
		
		protected abstract string OnGetWeaponPartDescription(string defaultLocalizationKey);
		

		public override ResourceCategory GetResourceCategory() {
			return ResourceCategory.WeaponParts;
		}

		public override string OnGroundVCPrefabName => EntityName;
		public override IResourceEntity GetReturnToBaseEntity() {
			return this;
		}

		public abstract WeaponPartType WeaponPartType { get; }


		public IWeaponPartsBuff OnGetBuff(IWeaponEntity weaponEntity) {
			return WeaponPartsBuff<T, TBuffType>.CreateBuff(this, weaponEntity);
		}

		public Type BuffType { get; } = typeof(TBuffType);



		public override void OnRecycle() {
			
		}
	}
}