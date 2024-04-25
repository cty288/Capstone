using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using MikroFramework.ActionKit;
using MikroFramework.Architecture;
using MikroFramework.Singletons;

namespace _02._Scripts.Runtime.PlayerTasks {
	public interface IPlayerTaskSystem : ISystem {
		public HashSet<PlayerTask> GetAllTasks();

		public void AddTask(PlayerTask task);
	}
	public class PlayerTaskSystemUpdateExecutor : MonoMikroSingleton<PlayerTaskSystemUpdateExecutor> {
		public Action OnUpdate = () => { };
		private void Update() {
			OnUpdate.Invoke();
		}
	}
	public struct OnAddPlayerTask {
		public PlayerTask Task;
	}

	public struct OnTaskCompleted {
		public PlayerTask Task;
	
	}
	public class PlayerTaskSystem : AbstractSystem, IPlayerTaskSystem {
		protected IPlayerTaskModel playerTaskModel;
		protected HashSet<PlayerTask> removedTasks = new HashSet<PlayerTask>();
		protected HashSet<PlayerTask> addedTasks = new HashSet<PlayerTask>();
		protected override void OnInit() {
			playerTaskModel = this.GetModel<IPlayerTaskModel>();
			PlayerTaskSystemUpdateExecutor.Singleton.OnUpdate += OnUpdate;
		}

		private void OnUpdate() {
			HashSet<PlayerTask> tasks = playerTaskModel.GetAllTasks();
			removedTasks.Clear();

			foreach (PlayerTask task in tasks) {
				if (task.IsSatisfied()) {
					removedTasks.Add(task);
					task.OnFinish();
					this.SendEvent<OnTaskCompleted>(new OnTaskCompleted() {
						Task = task
					});
				}
			}
			
			foreach (PlayerTask task in removedTasks) {
				tasks.Remove(task);
			}
			
			if (addedTasks.Count > 0) {
				foreach (PlayerTask task in addedTasks) {
					tasks.Add(task);
					this.SendEvent<OnAddPlayerTask>(new OnAddPlayerTask() {
						Task = task
					});
				}
				addedTasks.Clear();
			}
		}

		public HashSet<PlayerTask> GetAllTasks() {
			return new HashSet<PlayerTask>(playerTaskModel.GetAllTasks());
		}

		public void AddTask(PlayerTask task) {
			addedTasks.Add(task);
		}
	}
}