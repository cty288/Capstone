using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using Priority_Queue;
using Runtime.DataFramework.Entities;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace _02._Scripts.Runtime.BuffSystem {
	public interface IBuffSystem : ISystem {
		// add, remove, contains
		bool CanAddBuff(IEntity targetEntity, IBuff buff);
		
		bool AddBuff(IEntity targetEntity, IEntity buffDealer, IBuff buff);
		
		bool RemoveBuff<T>(IEntity targetEntity) where T : IBuff;
		
		bool RemoveBuff(IEntity targetEntity, Type buffType);
		
		bool RemoveBuff(IEntity targetEntity, Type buffType, bool recycle, out IBuff buff);
		
		public bool ContainsBuff(IEntity targetEntity, Type buffType, out IBuff buff);

		public bool ContainsBuff<T>(IEntity targetEntity, out IBuff buff) where T : IBuff;
		
		public void SendBuffUpdateEvent(IEntity targetEntity, IEntity buffDealer, IBuff buff, BuffUpdateEventType eventType);
		
		public List<IBuff> GetBuffs(IEntity targetEntity);
	}
	public class BuffSystemUpdateExecutor : MonoMikroSingleton<BuffSystemUpdateExecutor> {
		public Action OnUpdate = () => { };
		private void Update() {
			OnUpdate.Invoke();
		}
	}
	
	
	public class BuffSystem : AbstractSystem, IBuffSystem {
		private IBuffModel buffModel;
		
		private Dictionary<string, SimplePriorityQueue<IBuff, int>> buffQueue;
		
		//cached
		private List<string> entityIDsToRemove = new List<string>();
		private Dictionary<IBuff, string> buffsToRemove = new Dictionary<IBuff, string>();

		//life cycle: OnInitialize -> OnValidate -> (When Add) OnInitialize -> OnAwake -> OnStack -> OnStart -> OnTick -> OnEnd
		protected override void OnInit() {
			buffModel = this.GetModel<IBuffModel>();
			buffQueue = buffModel.BuffModelContainer.BuffQueue;
			
			InitModelOnStart();
			BuffSystemUpdateExecutor.Singleton.OnUpdate += OnUpdate;
		}
		
		protected async UniTask InitModelOnStart() {
			InitBuffModel();
			await UniTask.WaitForSeconds(0.1f);
			ForceSendAllBuffUpdateEvents();
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
						buff.OnInitialize(GlobalEntities.GetEntityAndModel(buff.BuffDealerID).Item1, entity, true);
					}

					entity.RegisterOnEntityRecycled(OnEntityRecycled);
				}
			}
			
			foreach (var id in idsToRemove) {
				buffModel.BuffModelContainer.RemoveAllBuffs(id);
			}
		}

		private void OnEntityRecycled(IEntity e) {
			e.UnRegisterOnEntityRecycled(OnEntityRecycled);
			buffModel.BuffModelContainer.RemoveAllBuffs(e.UUID);
		}

		public bool CanAddBuff(IEntity targetEntity, IBuff buff) {
			return targetEntity !=null && targetEntity.UUID!=null && buff.Validate() && targetEntity.OnValidateBuff(buff);
		}

		public bool AddBuff(IEntity targetEntity, IEntity buffDealer, IBuff buff) {
			if (!CanAddBuff(targetEntity, buff)) {
				return false;
			}

			buff.OnInitialize(buffDealer, targetEntity);
			buff.OnAwake();
			if (ContainsBuff(targetEntity, buff.GetType(), out IBuff existingBuff)) {
				if (existingBuff.OnStacked(buff)) {
					SendBuffUpdateEvent(targetEntity,  buffDealer, existingBuff, BuffUpdateEventType.OnUpdate);
					buff.OnEnd();
				}else {
					return false;
				}
			}
			else {
				if (!buffQueue.ContainsKey(targetEntity.UUID)) {
					targetEntity.RegisterOnEntityRecycled(OnEntityRecycled);
				}
				
				buffModel.BuffModelContainer.AddBuff(buff);
				buff.OnStart();
				SendBuffUpdateEvent(targetEntity, buffDealer, buff, BuffUpdateEventType.OnStart);
			}
			return true;
		}
		
		protected void ForceSendAllBuffUpdateEvents() {
			foreach (var buffQueue in buffQueue) {
				IEntity entity = GlobalEntities.GetEntityAndModel(buffQueue.Key).Item1;
				
				if (entity == null) {
					continue;
				}
				
				foreach (var buff in buffQueue.Value) {
					string dealerID = buff.BuffDealerID;
					IEntity dealer = GlobalEntities.GetEntityAndModel(dealerID).Item1;
					SendBuffUpdateEvent(entity, dealer, buff, BuffUpdateEventType.OnStart);
				}
			}
		}
		
		public void SendBuffUpdateEvent(IEntity targetEntity, IEntity buffDealer, IBuff buff, BuffUpdateEventType eventType) {
			targetEntity.OnBuffUpdate(buff, eventType);
			if (buffDealer != null) {
				buffDealer.OnDealtBuffUpdate(targetEntity, buff, eventType);
			}
		}

		public List<IBuff> GetBuffs(IEntity targetEntity) {
			List<IBuff> buffs = new List<IBuff>();
			if (buffQueue.TryGetValue(targetEntity.UUID, out SimplePriorityQueue<IBuff, int> queue)) {
				foreach (var buff in queue) {
					buffs.Add(buff);
				}
			}
			
			return buffs;
		}

		public bool RemoveBuff<T>(IEntity targetEntity) where T : IBuff {
			return RemoveBuff(targetEntity, typeof(T));
		}

		public bool RemoveBuff(IEntity targetEntity, Type buffType) {
			return RemoveBuff(targetEntity, buffType, true, out _);
		}
		
		

		public bool RemoveBuff(IEntity targetEntity, Type buffType, bool recycle, out IBuff buff) {
			if (buffModel.BuffModelContainer.RemoveBuff(buffType, targetEntity.UUID, out buff)) {
				IEntity dealer = GlobalEntities.GetEntityAndModel(buff.BuffDealerID).Item1;
				buff.AutoRecycleWhenEnd = recycle;
				buff.OnEnd();
				SendBuffUpdateEvent(targetEntity, dealer, buff, BuffUpdateEventType.OnEnd);
				
				return true;
			}

			return false;
		}

		private void OnUpdate() {
			entityIDsToRemove.Clear();
			buffsToRemove.Clear();
			
			
			string[] entityKeys = buffQueue.Keys.ToArray();
			foreach (string key in entityKeys) {
				var queue = buffQueue[key];
				
				IEntity entity = GlobalEntities.GetEntityAndModel(key).Item1;
				if (entity == null) {
					entityIDsToRemove.Add(key);
					continue;
				}
				
				IBuff[] buffs = queue.ToArray();
				
				foreach (var buff in buffs) {
					IEntity dealer = GlobalEntities.GetEntityAndModel(buff.BuffDealerID).Item1;
					if (buff.TickInterval > 0) {
						buff.TickTimer -= Time.deltaTime;
						
						if (buff.TickTimer <= 0) {
							buff.TickTimer = buff.TickInterval;
							BuffStatus status = buff.OnTick();
							
							if (status == BuffStatus.End) {
								
								buffsToRemove.TryAdd(buff, buff.BuffOwnerID);
								SendBuffUpdateEvent(entity, dealer, buff, BuffUpdateEventType.OnEnd);
								buff.OnEnd();
							}
							else {
								SendBuffUpdateEvent(entity, dealer, buff, BuffUpdateEventType.OnUpdate);
							}
						}
					}
					
					if (buff.MaxDuration > 0 && !buffsToRemove.ContainsKey(buff)) {
						buff.RemainingDuration = Mathf.Max(0, buff.RemainingDuration - Time.deltaTime);
						if (buff.RemainingDuration == 0) {
							
							buffsToRemove.TryAdd(buff, buff.BuffOwnerID);
							SendBuffUpdateEvent(entity, dealer, buff, BuffUpdateEventType.OnEnd);
							buff.OnEnd();
						}
					}
				}
			}
			
			
			
			foreach (var id in entityIDsToRemove) {
				buffModel.BuffModelContainer.RemoveAllBuffs(id);
			}
			
			foreach (var buff in buffsToRemove) {
				buffModel.BuffModelContainer.RemoveBuff(buff.Value, buff.Key);
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