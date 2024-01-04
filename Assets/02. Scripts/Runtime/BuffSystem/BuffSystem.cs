using System;
using System.Collections.Generic;
using MikroFramework.Architecture;
using PriorityQueues;
using Runtime.DataFramework.Entities;

namespace _02._Scripts.Runtime.BuffSystem {
	public interface IBuffSystem : ISystem {
		// add, remove, contains
		bool CanAddBuff(IEntity targetEntity, IBuff buff);
		
		bool AddBuff(IEntity targetEntity, IBuff buff);
		
		bool RemoveBuff<T>(IEntity targetEntity) where T : IBuff;
		
		public bool ContainsBuff(IEntity targetEntity, Type buffType, out IBuff buff);

		public bool ContainsBuff<T>(IEntity targetEntity, out IBuff buff) where T : IBuff;
	}
	public class BuffSystem : AbstractSystem, IBuffSystem {
		private IBuffModel buffModel;
		
		
		//life cycle: OnInitialize -> OnValidate -> OnAwake -> OnStack -> OnStart -> OnTick -> OnEnd
		protected override void OnInit() {
			buffModel = this.GetModel<IBuffModel>();
			InitBuffModel();
		}

		private void InitBuffModel() {
			Dictionary<string, MappedBinaryPriorityQueue<IBuff>> BuffQueue = buffModel.BuffModelContainer.BuffQueue;
			
			HashSet<string> idsToRemove = new HashSet<string>();
			foreach (var buffQueue in BuffQueue) {
				IEntity entity = GlobalEntities.GetEntityAndModel(buffQueue.Key).Item1;
				if (entity == null) {
					idsToRemove.Add(buffQueue.Key);
				}
				else {
					foreach (var buff in buffQueue.Value) {
						buff.OnInitialize(GlobalEntities.GetEntityAndModel(buff.BuffDealerID).Item1, entity);
					}
				}
			}
			
			foreach (var id in idsToRemove) {
				buffModel.BuffModelContainer.RemoveAllBuffs(id);
			}
		}

		public bool CanAddBuff(IEntity targetEntity, IBuff buff) {
			return buff.Validate() && targetEntity.OnValidateBuff(buff);
		}

		public bool AddBuff(IEntity targetEntity, IBuff buff) {
			if (!CanAddBuff(targetEntity, buff)) {
				return false;
			}

			buff.OnAwake();
			if (ContainsBuff(targetEntity, buff.GetType(), out IBuff existingBuff)) {
				existingBuff.OnStacked(buff);
			}
			else {
				buffModel.BuffModelContainer.AddBuff(buff);
				buff.OnStart();
			}
			return true;
		}

		public bool RemoveBuff<T>(IEntity targetEntity) where T : IBuff {
			if (buffModel.BuffModelContainer.RemoveBuff<T>(targetEntity.UUID, out IBuff removedBuff)) {
				removedBuff.OnEnd();
				return true;
			}

			return false;
		}

		public bool ContainsBuff<T>(IEntity targetEntity, out IBuff buff) where T : IBuff {
			return buffModel.BuffModelContainer.HasBuff<T>(targetEntity.UUID, out buff);
		}
		
		public bool ContainsBuff(IEntity targetEntity, Type buffType, out IBuff buff) {
			return buffModel.BuffModelContainer.HasBuff(targetEntity.UUID, buffType, out buff);
		}
	}
}