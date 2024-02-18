using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.Pool;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace _02._Scripts.Runtime.BuffSystem.ConfigurableBuff {
	public abstract class ConfigurableBuff<TBuff> : PropertyBuff<TBuff>, IBuff, ILeveledBuff where TBuff : ConfigurableBuff<TBuff>, new() {
		[field: ES3Serializable] 
		public int Level { get; protected set; }

		[field: ES3Serializable]
		public int MaxLevel { get; protected set; }

		public abstract string GetLevelDescription(int level);
		

		public override string OnGetDescription(string defaultLocalizationKey) {
			return GetLevelDescription(Level);
		}

		private static Dictionary<string, Dictionary<int, Dictionary<string, dynamic>>> _globalBuffLevelProperties =
			new Dictionary<string, Dictionary<int, Dictionary<string, dynamic>>>();

		//private Dictionary<int, Dictionary<string, dynamic>> _buffLevelProperties;


		static ConfigurableBuff() {
			ReadBuffLevelProperties();
		}
		
		public void LevelUp() {
			LevelUp(1);
		}
		
		public void LevelUp(int level) {
			int actualLevelUpCount = Math.Min(level, MaxLevel - Level);
			Level += actualLevelUpCount;
			buffOwner?.OnBuffUpdate(this, BuffUpdateEventType.OnUpdate);
			OnLevelUp();
		}
		
		protected abstract void OnLevelUp();

		public ConfigurableBuff() {
			//_buffLevelProperties = _globalBuffLevelProperties[GetType().Name];
		}
		
		protected T GetBuffPropertyAtLevel<T>(string propertyName, int level) {
			return GetBuffPropertyAtLevel<T>(GetType().Name, propertyName, level);
		}
		
		protected T GetBuffPropertyAtCurrentLevel<T>(string propertyName) {
			return GetBuffPropertyAtLevel<T>(propertyName, Level);
		}

		public static T GetBuffPropertyAtLevel<T>(string buffName, string propertyName, int level) {
			if (_globalBuffLevelProperties.ContainsKey(buffName)) {
				Dictionary<int, Dictionary<string, dynamic>> buffLevelProperties = _globalBuffLevelProperties[buffName];
				
				if (buffLevelProperties.ContainsKey(level)) {
					if (buffLevelProperties[level].ContainsKey(propertyName)) {
						return (T) buffLevelProperties[level][propertyName];
					}
				}
				
				
				//find the cloest level that is <= level that has the property
				int closestLevel = 0;
				foreach (int l in buffLevelProperties.Keys) {
					if (l <= level && l > closestLevel) {
						if (buffLevelProperties[l].ContainsKey(propertyName)) {
							closestLevel = l;
						}
					}
				}
				
				if (buffLevelProperties.ContainsKey(closestLevel)) {
					if (buffLevelProperties[closestLevel].ContainsKey(propertyName)) {
						return (T) buffLevelProperties[closestLevel][propertyName];
					}
				}
			}
			return default;
		}
		
	
		
		private static void ReadBuffLevelProperties() {
			string[] buffNames = ConfigDatas.Singleton.BuffConfigTable.GetKeys();
			foreach (string buffName in buffNames) {
				_globalBuffLevelProperties.Add(buffName, new Dictionary<int, Dictionary<string, dynamic>>());

				Dictionary<int, Dictionary<string, dynamic>> buffLevelProperties = _globalBuffLevelProperties[buffName];
				
				dynamic rawData = ConfigDatas.Singleton.BuffConfigTable.Get<dynamic>(buffName, "level_properties");
				IEnumerable<string> rawKeys = (rawData as JObject)?.Properties().Select(p => p.Name);
			
				if (rawKeys != null) {
					foreach (string key in rawKeys) {
						//key should be "levelx". x is the level number, get x
						int level = int.Parse(key.Substring(5));
						buffLevelProperties.Add(level, new Dictionary<string, dynamic>());
					
						dynamic value = rawData[key];
						IEnumerable<string> keys = (value as JObject)?.Properties().Select(p => p.Name);
					
						foreach (string k in keys) {
							string type = value[k]["type"];
							Type parsedType = SerializationFactory.Singleton.ParseType(type);
							string rawVal = value[k]["value"].ToString();
					
						
							dynamic result = null;
							if (parsedType == typeof(string)) {
								result = rawVal;
							}
							else if (parsedType == typeof(object)) {
								result = CustomDataProperty<object>.ConvertJTokenToBaseValue(value[k]["value"]);
							}
							else {
								result = JsonConvert.DeserializeObject(rawVal, parsedType);
							}

							buffLevelProperties[level].Add(k, result);

						}
					}
				}
			}
			

		}


		public override bool OnStacked(TBuff buff) {
			int oldLevel = this.Level;
			if (buff.Level > this.Level) {
				this.Level = buff.Level;
			}

			OnBuffStacked(buff);
			if (oldLevel < this.Level) {
				OnLevelUp();
			}

			return true;
		}
		
		
		protected abstract void OnBuffStacked(TBuff buff);

		public static TBuff Allocate(IEntity buffDealer, IEntity entity, int level) {
			TBuff buff = SafeObjectPool<TBuff>.Singleton.Allocate();
			buff.OnInitialize(buffDealer, entity);
			buff.Level = level;
			buff.MaxLevel = ConfigDatas.Singleton.BuffConfigTable.Get<int>(buff.GetType().Name, "max_level");
			return buff;
		}
	}
}