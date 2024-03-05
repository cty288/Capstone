using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using UnityEngine;

public interface IGameEventSystem : ISystem {
	string AddEvent(IGameEvent gameEvent, int remainingMinutesToTrigger);
	void RemoveEvent(IGameEvent gameEvent);
}
public class GameEventSystem : AbstractSystem, IGameEventSystem {
	private IGameEventModel gameEventModel;
	private IGameTimeModel gameTimeModel;
	private ILevelModel levelModel;
	private HashSet<IGameEvent> eventsToRemoved = new HashSet<IGameEvent>();
	protected override void OnInit() {
		gameEventModel = this.GetModel<IGameEventModel>();
		gameTimeModel = this.GetModel<IGameTimeModel>();
		levelModel = this.GetModel<ILevelModel>();
		gameTimeModel.GlobalTime.RegisterOnValueChanged(OnGlobalTimeChanged);
		levelModel.CurrentLevelCount.RegisterOnValueChanged(OnLevelCountChanged);
		gameEventModel.InitExistingEvents();
	}

	private void OnLevelCountChanged(int oldLevel, int newLevel) {
		if (oldLevel == newLevel) {
			return;
		}
		eventsToRemoved.Clear();
		
		foreach (IGameEvent gameEvent in gameEventModel.GameEvents.Values) {
			if (gameEvent.StartWithLevel == oldLevel && !gameEvent.CanPersistToOtherLevels) {
				eventsToRemoved.Add(gameEvent);
				gameEvent.OnEventLeaped();
			}
		}

		foreach (IGameEvent gameEvent in eventsToRemoved) {
			gameEventModel.RemoveEvent(gameEvent);
		}
		
		Debug.Log("GameEventSystem.OnLevelCountChanged");
	}

	private void OnGlobalTimeChanged(DateTime oldTime, DateTime newTime) {
		eventsToRemoved.Clear();
		int minuteElapsed = (int) (newTime - oldTime).TotalMinutes;
		foreach (IGameEvent gameEvent in gameEventModel.GameEvents.Values) {
			Debug.LogWarning(gameEvent);
			int realMinuteElapsed = gameEvent.ElapseType == EventElapseType.Predetermined
				? minuteElapsed
				: Mathf.Clamp(minuteElapsed, 0, 1);
			
			gameEvent.RemainingMinutesToTrigger -= realMinuteElapsed;
			if (gameEvent.RemainingMinutesToTrigger is 0 or -1) {
				gameEvent.OnEventTriggered();
				eventsToRemoved.Add(gameEvent);
			}else if (gameEvent.RemainingMinutesToTrigger < -1) {
				gameEvent.OnEventLeaped();
				eventsToRemoved.Add(gameEvent);
			}
		}
		
		foreach (IGameEvent gameEvent in eventsToRemoved) {
			gameEventModel.RemoveEvent(gameEvent);
		}
		
	}

	public string AddEvent(IGameEvent gameEvent, int remainingMinutesToTrigger) {
		gameEvent.StartWithLevel = levelModel.CurrentLevelCount.Value;
		gameEventModel.AddEvent(gameEvent, remainingMinutesToTrigger);
		return gameEvent.EventID;
	}

	public void RemoveEvent(IGameEvent gameEvent) {
		gameEventModel.RemoveEvent(gameEvent);
	}
}
