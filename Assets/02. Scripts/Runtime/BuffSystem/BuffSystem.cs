using System;
using System.Collections.Generic;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using PriorityQueues;
using Runtime.DataFramework.Entities;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace _02._Scripts.Runtime.BuffSystem {
	public interface IBuffSystem : ISystem {
		// add, remove, contains
		bool CanAddBuff(IEntity targetEntity, IBuff buff);
		
		bool AddBuff(IEntity targetEntity, IBuff buff);
		
		bool RemoveBuff<T>(IEntity targetEntity) where T : IBuff;
		
		bool RemoveBuff(IEntity targetEntity, Type buffType);
		
		public bool ContainsBuff(IEntity targetEntity, Type buffType, out IBuff buff);

		public bool ContainsBuff<T>(IEntity targetEntity, out IBuff buff) where T : IBuff;
	}
	public class BuffSystemUpdateExecutor : MonoMikroSingleton<BuffSystemUpdateExecutor> {
		public Action OnUpdate = () => { };
		private void Update() {
			OnUpdate.Invoke();
		}
	}
	
	
	public class BuffSystem : AbstractSystem, IBuffSystem {
		private IBuffModel buffModel;
		
		private Dictionary<string, MappedBinaryPriorityQueue<IBuff>> buffQueue;
		
		//cached
		private List<string> entityIDsToRemove = new List<string>();
		private HashSet<IBuff> buffsToRemove = new HashSet<IBuff>();

		//life cycle: OnInitialize -> OnValidate -> OnAwake -> OnStack -> OnStart -> OnTick -> OnEnd
		protected override void OnInit() {
			buffModel = this.GetModel<IBuffModel>();
			buffQueue = buffModel.BuffModelContainer.BuffQueue;
			InitBuffModel();
			BuffSystemUpdateExecutor.Singleton.OnUpdate += OnUpdate;
		}

		

		private void InitBuffModel() {
			
			HashSet<string> idsToRemove = new HashSet<string>();
			foreach (var buffQueue in buffQueue) {
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
			return targetEntity !=null && buff.Validate() && targetEntity.OnValidateBuff(buff);
		}

		public bool AddBuff(IEntity targetEntity, IBuff buff) {
			if (!CanAddBuff(targetEntity, buff)) {
				return false;
			}

			buff.OnAwake();
			if (ContainsBuff(targetEntity, buff.GetType(), out IBuff existingBuff)) {
				existingBuff.OnStacked(buff);
				SendBuffUpdateEvent(targetEntity, existingBuff, BuffUpdateEventType.OnUpdate);
			}
			else {
				buffModel.BuffModelContainer.AddBuff(buff);
				buff.OnStart();
				SendBuffUpdateEvent(targetEntity, buff, BuffUpdateEventType.OnStart);
			}
			return true;
		}
		
		protected void SendBuffUpdateEvent(IEntity targetEntity, IBuff buff, BuffUpdateEventType eventType) {
			targetEntity.OnBuffUpdate(buff, eventType);
		}

		public bool RemoveBuff<T>(IEntity targetEntity) where T : IBuff {
			return RemoveBuff(targetEntity, typeof(T));
		}

		public bool RemoveBuff(IEntity targetEntity, Type buffType) {
			if (buffModel.BuffModelContainer.RemoveBuff(buffType, targetEntity.UUID, out IBuff removedBuff)) {
				removedBuff.OnEnd();
				SendBuffUpdateEvent(targetEntity, removedBuff, BuffUpdateEventType.OnEnd);
				return true;
			}

			return false;
		}

		private void OnUpdate() {
			entityIDsToRemove.Clear();
			buffsToRemove.Clear();
			
			foreach (var buffQueue in buffQueue) {
				IEntity entity = GlobalEntities.GetEntityAndModel(buffQueue.Key).Item1;
				if (entity == null) {
					entityIDsToRemove.Add(buffQueue.Key);
					continue;
				}
				
				foreach (var buff in buffQueue.Value) {
					if (buff.TickInterval > 0) {
						buff.TickTimer -= Time.deltaTime;
						
						if (buff.TickTimer <= 0) {
							buff.TickTimer = buff.TickInterval;
							BuffStatus status = buff.OnTick();
							
							if (status == BuffStatus.End) {
								buff.OnEnd();
								buffsToRemove.Add(buff);
								SendBuffUpdateEvent(entity, buff, BuffUpdateEventType.OnEnd);
							}
							else {
								SendBuffUpdateEvent(entity, buff, BuffUpdateEventType.OnUpdate);
							}
						}
					}
					
					if (buff.MaxDuration > 0 && !buffsToRemove.Contains(buff)) {
						buff.RemainingDuration = Mathf.Max(0, buff.RemainingDuration - Time.deltaTime);
						if (buff.RemainingDuration == 0) {
							buff.OnEnd();
							buffsToRemove.Add(buff);
							SendBuffUpdateEvent(entity, buff, BuffUpdateEventType.OnEnd);
						}
					}
				}
			}
			
			foreach (var id in entityIDsToRemove) {
				buffModel.BuffModelContainer.RemoveAllBuffs(id);
			}
			
			foreach (var buff in buffsToRemove) {
				buffModel.BuffModelContainer.RemoveBuff(buff);
			}
		}

		public bool ContainsBuff<T>(IEntity targetEntity, out IBuff buff) where T : IBuff {
			return buffModel.BuffModelContainer.HasBuff<T>(targetEntity.UUID, out buff);
		}
		
		public bool ContainsBuff(IEntity targetEntity, Type buffType, out IBuff buff) {
			return buffModel.BuffModelContainer.HasBuff(targetEntity.UUID, buffType, out buff);
		}
	}
}