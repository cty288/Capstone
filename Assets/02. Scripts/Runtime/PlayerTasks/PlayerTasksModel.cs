using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using Framework;

namespace _02._Scripts.Runtime.PlayerTasks {
	public interface IPlayerTaskModel : ISavableModel {
		public HashSet<PlayerTask> GetAllTasks();
		
	}
	public class PlayerTasksModel : AbstractSavableModel, IPlayerTaskModel{
		[field: ES3Serializable]
		private HashSet<PlayerTask> playerTasks = new HashSet<PlayerTask>();


		public HashSet<PlayerTask> GetAllTasks() {
			return playerTasks;
		}
	}
}