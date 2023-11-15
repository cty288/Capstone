using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.Model.Properties;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Properties;
using MikroFramework.Singletons;
using Runtime.DataFramework.Properties.TagProperty;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources.Model.Properties.BaitAdjectives;
using Runtime.Weapons.Model.Properties;
using UnityEngine;

namespace Runtime.Utilities.ConfigSheet {
	public class SerializationFactory : MikroSingleton<SerializationFactory>, ISingleton {
		private SerializationFactory(){}
		private readonly Dictionary<string, Type> _typeMappings = new Dictionary<string, Type>();

		//public Dictionary<string, Type> TypeMappings => _typeMappings;
		
		public override void OnSingletonInit() {
			base.OnSingletonInit();
			OnRegisterTypes();
		}

		//no need to register generic types
		private void OnRegisterTypes() {
			RegisterType("string", typeof(string));
			RegisterType("int", typeof(int));
			RegisterType("float", typeof(float));
			RegisterType("bool", typeof(bool));
			RegisterType("long", typeof(long));
			RegisterType("double", typeof(double));
			RegisterType("short", typeof(short));
			RegisterType("dynamic", typeof(object));
			RegisterType("Vector2", typeof(Vector2));
			RegisterType("Vector3", typeof(Vector3));
			RegisterType("Vector4", typeof(Vector4));
			RegisterType("Vector2Int", typeof(Vector2Int));
			RegisterType("Vector3Int", typeof(Vector3Int));


			RegisterType("HealthInfo", typeof(HealthInfo));
			RegisterType("TasteType", typeof(TasteType));
			RegisterType("TagName", typeof(TagName));
			RegisterType("BaitAdjective", typeof(BaitAdjective));
			RegisterType("RecoilInfo", typeof(RecoilInfo));
			RegisterType("ItemDropCollection", typeof(ItemDropCollection));
			RegisterType("CurrencyType", typeof(CurrencyType));
			RegisterType("SkillPurchaseCostInfo", typeof(SkillPurchaseCostInfo));
			RegisterType("CollectableResourceCurrencyInfo", typeof(CollectableResourceCurrencyInfo));

		}
		
		
		public void RegisterType(string alias, Type actualType) {
			_typeMappings[alias] = actualType;
		}
		

		public Type ParseType(string typeName) {
			if (typeName.EndsWith("[]")) {
				string elementType = typeName.Substring(0, typeName.Length - 2);
				return ParseType(elementType).MakeArrayType();
			}

			if (_typeMappings.TryGetValue(typeName, out Type actualType))
			{
				return actualType;
			}


			if (typeName.StartsWith("Dictionary<")) return CreateGenericType(typeof(Dictionary<,>), typeName);
			if (typeName.StartsWith("List<")) return CreateGenericType(typeof(List<>), typeName);
			if (typeName.StartsWith("HashSet<")) return CreateGenericType(typeof(HashSet<>), typeName);

			return typeof(object);
		}

		private Type CreateGenericType(Type genericTypeDefinition, string typeName)
		{
			int innerStart = typeName.IndexOf('<') + 1;
			int innerEnd = typeName.LastIndexOf('>');
			var innerTypeName = typeName.Substring(innerStart, innerEnd - innerStart);

			var typeArgs = new List<Type>();
			int nestLevel = 0;
			int argStart = 0;
			for (int i = 0; i < innerTypeName.Length; i++)
			{
				char c = innerTypeName[i];
				if (c == '<') nestLevel++;
				if (c == '>') nestLevel--;
				if (c == ',' && nestLevel == 0)
				{
					typeArgs.Add(ParseType(innerTypeName.Substring(argStart, i - argStart).Trim()));
					argStart = i + 1;
				}
			}

			typeArgs.Add(ParseType(innerTypeName.Substring(argStart).Trim()));

			return genericTypeDefinition.MakeGenericType(typeArgs.ToArray());
		}
	}
}