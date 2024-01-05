using System;
using System.Collections.Generic;
using System.Linq;
using PriorityQueues;
using Runtime.DataFramework.Entities;

namespace _02._Scripts.Runtime.BuffSystem {
	[Serializable]
	public class BuffModelContainer {
		[ES3Serializable]
		private Dictionary<string, HashSet<IBuff>> _buffs = new Dictionary<string, HashSet<IBuff>>();

		[ES3NonSerializable]
		private Dictionary<string, MappedBinaryPriorityQueue<IBuff>> _buffQueue =
			new Dictionary<string, MappedBinaryPriorityQueue<IBuff>>();

		public Dictionary<string, MappedBinaryPriorityQueue<IBuff>> BuffQueue => _buffQueue;

		[ES3NonSerializable]
		//cache the type of buff for each entity
		private Dictionary<string, Dictionary<Type, IBuff>> _buffTypeMap =
			new Dictionary<string, Dictionary<Type, IBuff>>();

		public void InitFromSave() {
			foreach (var buffList in _buffs.Values) {
				foreach (var buff in buffList) {
					AddBuff(buff);
				}
			}
		}

		public void AddBuff(IBuff buff) {
			if (!_buffs.ContainsKey(buff.BuffOwnerID)) {
				_buffs.Add(buff.BuffOwnerID, new HashSet<IBuff>());
			}
			

			_buffs[buff.BuffOwnerID].Add(buff);

			if (!_buffQueue.ContainsKey(buff.BuffOwnerID)) {
				_buffQueue.Add(buff.BuffOwnerID,
					new MappedBinaryPriorityQueue<IBuff>((a, b) => a.Priority.CompareTo(b.Priority)));
			}

			_buffQueue[buff.BuffOwnerID].Enqueue(buff);
			
			if (!_buffTypeMap.ContainsKey(buff.BuffOwnerID)) {
				_buffTypeMap.Add(buff.BuffOwnerID, new Dictionary<Type, IBuff>());
			}

			_buffTypeMap[buff.BuffOwnerID].Add(buff.GetType(), buff);
		}
		
		
		public bool RemoveBuff<T>(string buffOwnerID, out IBuff removedBuff) where T : IBuff {
			return RemoveBuff(typeof(T), buffOwnerID, out removedBuff);
		}
		
		public bool RemoveBuff(Type buffType, string buffOwnerID, out IBuff removedBuff) {
			removedBuff = null;
			if (!_buffs.ContainsKey(buffOwnerID)) {
				return false;
			}

			if (!_buffTypeMap.ContainsKey(buffOwnerID)) {
				return false;
			}

			if (!_buffTypeMap[buffOwnerID].ContainsKey(buffType)) {
				return false;
			}

			removedBuff = _buffTypeMap[buffOwnerID][buffType];
			_buffs[buffOwnerID].Remove(removedBuff);
			_buffQueue[buffOwnerID].Remove(removedBuff);
			_buffTypeMap[buffOwnerID].Remove(buffType);
			
			if (_buffs[buffOwnerID].Count == 0) {
				_buffs.Remove(buffOwnerID);
				_buffQueue.Remove(buffOwnerID);
				_buffTypeMap.Remove(buffOwnerID);
			}
			return true;
		}
		
		public bool RemoveBuff(IBuff buff) {
			return RemoveBuff(buff.GetType(), buff.BuffOwnerID, out _);
		}
		
		public bool HasBuff<T>(string buffOwnerID, out IBuff buff) where T : IBuff {
			return HasBuff(buffOwnerID, typeof(T), out buff);
		}
		
		public bool HasBuff(string buffOwnerID, Type buffType, out IBuff buff) {
			buff = null;
			if (!_buffs.ContainsKey(buffOwnerID)) {
				return false;
			}

			if (!_buffTypeMap.ContainsKey(buffOwnerID)) {
				return false;
			}


			if (_buffTypeMap[buffOwnerID].ContainsKey(buffType)) {
				buff = _buffTypeMap[buffOwnerID][buffType];
				return true;
			}
			
			return false;
		}
		
		public bool RemoveAllBuffs(string buffOwnerID) {
			if (!_buffs.ContainsKey(buffOwnerID)) {
				return false;
			}

			if (!_buffTypeMap.ContainsKey(buffOwnerID)) {
				return false;
			}

			_buffs[buffOwnerID].Clear();
			_buffQueue[buffOwnerID].Clear();
			_buffTypeMap[buffOwnerID].Clear();
			_buffs.Remove(buffOwnerID);
			_buffQueue.Remove(buffOwnerID);
			_buffTypeMap.Remove(buffOwnerID);
			
			return true;
		}
		
		public HashSet<string> GetAllBuffOwnerIDs() {
			return _buffs.Keys.ToHashSet();
		}


	}
}