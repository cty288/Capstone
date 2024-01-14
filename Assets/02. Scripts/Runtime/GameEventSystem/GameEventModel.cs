using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public interface IGameEventModel : ISavableModel {
	void InitExistingEvents();
	
	void AddEvent(IGameEvent gameEvent, int remainingMinutesToTrigger);
	
	void RemoveEvent(IGameEvent gameEvent);
	
	HashSet<IGameEvent> GameEvents { get; }
}
public class GameEventModel : AbstractSavableModel, IGameEventModel {

	[field: ES3Serializable]
	private HashSet<IGameEvent> gameEvents = new HashSet<IGameEvent>();
	
	protected override void OnInit() {
		base.OnInit();
	}

	public void InitExistingEvents() {
		foreach (var gameEvent in gameEvents) {
			gameEvent.OnInitialize();
		}
	}

	public void AddEvent(IGameEvent gameEvent, int remainingMinutesToTrigger) {
		gameEvent.RemainingMinutesToTrigger = remainingMinutesToTrigger;
		gameEvent.OnInitialize();
		gameEvents.Add(gameEvent);
	}

	public void RemoveEvent(IGameEvent gameEvent) {
		gameEvents.Remove(gameEvent);
		gameEvent.RecycleToCache();
	}

	public HashSet<IGameEvent> GameEvents => gameEvents;
}
