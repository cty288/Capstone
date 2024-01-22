using System;
using MikroFramework.BindableProperty;
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
		
		public BindableProperty<T> GetCustomDataValueWithLevel<T>(string propertyName, int level);

		public BindableProperty<T> GetCustomDataValueOfCurrentLevel<T>(string propertyName);
		
		public Type BuffType { get;}
	}
	
	public abstract class WeaponPartsEntity<T, TBuffType> : BuildableResourceEntity<T>, IWeaponPartsEntity
		where T : WeaponPartsEntity<T, TBuffType>, new()
		where TBuffType : WeaponPartsBuff<T, TBuffType>, new() {
		
		protected virtual int levelRange => 4;
		protected override ConfigTable GetConfigTable() {
			return ConfigDatas.Singleton.WeaponPartsConfigTable;
		}

		public override int GetMaxRarity() {
			return 4;
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
			return null;
		}

		public abstract WeaponPartType WeaponPartType { get; }


		public IWeaponPartsBuff OnGetBuff(IWeaponEntity weaponEntity) {
			return WeaponPartsBuff<T, TBuffType>.CreateBuff(this, weaponEntity);
		}

		

		public Type BuffType { get; } = typeof(TBuffType);



		public override void OnRecycle() {
			
		}
		
		
		
		public BindableProperty<T1> GetCustomDataValueWithLevel<T1>(string propertyName, int level) {
			int targetLevel = level + 1;
			while (targetLevel > 0) {
				targetLevel--;
				string name = $"level{targetLevel}";
				if (!HasCustomProperty(name)) {
					continue;
				}

				if (GetCustomProperties()[name].TryGetCustomDataValue<T1>(propertyName, out BindableProperty<T1> val)) {
					return val;
				}
			}
			return null;
		}

		public BindableProperty<T1> GetCustomDataValueOfCurrentLevel<T1>(string propertyName) {
			return GetCustomDataValueWithLevel<T1>(propertyName, GetRarity());
		}
		

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			AutoConfigCustomProperty[] properties = new AutoConfigCustomProperty[levelRange];
			for (int i = 1; i <= levelRange; i++) {
				properties[i - 1] = new AutoConfigCustomProperty($"level{i}");
			}
			
			ICustomProperty[] additionalProperties = OnRegisterAdditionalCustomProperties();
			
			if (additionalProperties != null) {
				ICustomProperty[] result = new ICustomProperty[properties.Length + additionalProperties.Length];
				properties.CopyTo(result, 0);
				additionalProperties.CopyTo(result, properties.Length);
				return result;
			}

			ICustomProperty[] res= new ICustomProperty[properties.Length];
			properties.CopyTo(res, 0);
			return res;
		}

		protected abstract ICustomProperty[] OnRegisterAdditionalCustomProperties();
	}
}