using System;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using Runtime.Player;
using UnityEngine;

namespace _02._Scripts.Runtime.TimeSystem {
	public interface IGameTimeSystem : ISystem {
		public float speed_debug { get; set; }
	}
	public class GameTimeSystemUpdateExecutor : MonoMikroSingleton<GameTimeSystemUpdateExecutor> {
		public Action OnUpdate = () => { };
		private void Update() {
			OnUpdate.Invoke();
		}
	}
	
	public class GameTimeSystem : AbstractSystem, IGameTimeSystem {
		private IGameTimeModel gameTimeModel;
		private ILevelModel levelModel;
		private IGamePlayerModel playerModel;
		private float realWorldSecondPerGameMinute = 1;
		private float timer = 0;
		
		public float speed_debug { get; set; } = 1;
		protected override void OnInit() {
			gameTimeModel = this.GetModel<IGameTimeModel>();
			levelModel = this.GetModel<ILevelModel>();
			playerModel = this.GetModel<IGamePlayerModel>();
			//1440 minutes in a day
			realWorldSecondPerGameMinute = GameTimeModel.DayLength / 1440f;
			GameTimeSystemUpdateExecutor.Singleton.OnUpdate += OnUpdate;

			levelModel.CurrentLevelCount.RegisterOnValueChanged(OnLevelCountChanged);
		}

		private void OnLevelCountChanged(int previousLevelNum, int currentLevelNum) {
			if (previousLevelNum == currentLevelNum) {
				return;
			}
			if (previousLevelNum == 0) {
				this.SendEvent<OnNewDayStart>(new OnNewDayStart() {
					DayCount = 1
				});
				return; //do nothing when the change level from base
			}

			if (currentLevelNum == 0) {
				gameTimeModel.NewRound();
			}
			else {
				gameTimeModel.NextDay();
			}
			
			Debug.Log("GameTimeSystem.OnLevelCountChanged");
		}

		private void OnUpdate() {
			//time does not change in base
			if (levelModel.CurrentLevelCount == 0 || playerModel.IsPlayerDead()) {
				return;
			}
			
			timer += Time.deltaTime;
			if (timer >= realWorldSecondPerGameMinute / speed_debug) {
				timer = 0;
				gameTimeModel.NextMinute();
			}
		}
	}
}