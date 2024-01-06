﻿using System;
using System.Collections.Generic;
using System.Reflection;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Builders;
using Framework;
using Runtime.DataFramework.Entities.Builders;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;

namespace _02._Scripts.Runtime.WeaponParts.Model {
	public interface IWeaponPartsModel : IGameResourceModel<IWeaponPartsEntity>, ISavableModel {
		IEntityBuilder GetWeaponPartsBuilder(string weaponEntityTypeName, bool addToModelOnceBuilt = true);
	}
	
	public class WeaponPartsModel  : GameResourceModel<IWeaponPartsEntity>, IWeaponPartsModel {
		[ES3NonSerializable]
		private Dictionary<string, MethodInfo> builderMethods = new Dictionary<string, MethodInfo>();
		protected override void OnInit() {
			base.OnInit();
			RegisterBuilders();
		}

		public IEntityBuilder GetWeaponPartsBuilderBasic<T>(bool addToModelOnceBuilt = true) where T : class, IWeaponPartsEntity, new() {
			WeaponPartsBuilder<T> builder = entityBuilderFactory.GetBuilder<WeaponPartsBuilder<T>, T>(1);
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}
			return builder;
		}

		private void RegisterBuilders() {
			//get all the types that implement IWeaponPartsEntity
			var types = typeof(IWeaponPartsEntity).Assembly.GetTypes();
			foreach (var type in types) {
				if (type.IsAbstract || type.IsInterface) {
					continue;
				}

				if (typeof(IWeaponPartsEntity).IsAssignableFrom(type)) {
					//register the builder


					MethodInfo builderMethod =
						this.GetType().GetMethod("GetWeaponPartsBuilderBasic")?.MakeGenericMethod(type);

					builderMethods.Add(type.Name, builderMethod);

				}
			}
		}


		public IEntityBuilder GetWeaponPartsBuilder(string weaponEntityTypeName, bool addToModelOnceBuilt = true) {
			if (builderMethods.TryGetValue(weaponEntityTypeName, out var method)) {
				return method.Invoke(this, new object[] {addToModelOnceBuilt}) as IEntityBuilder;
			}

			throw new Exception("Cannot find the builder for the weapon entity type: " + weaponEntityTypeName);
		}
	}
}