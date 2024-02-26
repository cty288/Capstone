using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public interface IGameEventModel : ISavableModel {
	void InitExistingEvents();
	
	void AddEvent(IGameEvent gameEvent, int remainingMinutesToTrigger);
	
	void RemoveEvent(IGameEvent gameEvent);
	
	Dictionary<string, IGameEvent> GameEvents { get; }
	
	IGameEvent GetEvent(string eventID);
}
public class GameEventModel : AbstractSavableModel, IGameEventModel {

	[field: ES3Serializable]
	private Dictionary<string, IGameEvent> gameEvents = new Dictionary<string, IGameEvent>();
	
	protected override void OnInit() {
		base.OnInit();
	}

	public void InitExistingEvents() {
		foreach (var gameEvent in gameEvents) {
			gameEvent.Value.OnInitialize();
		}
	}

	public void AddEvent(IGameEvent gameEvent, int remainingMinutesToTrigger) {
		gameEvent.EventID = System.Guid.NewGuid().ToString();
		gameEvent.RemainingMinutesToTrigger = remainingMinutesToTrigger;
		gameEvent.TotalMinutes = remainingMinutesToTrigger;
		gameEvent.OnInitialize();
		
		gameEvents.Add(gameEvent.EventID, gameEvent);
	}

	public void RemoveEvent(IGameEvent gameEvent) {
		gameEvents.Remove(gameEvent.EventID);
		gameEvent.RecycleToCache();
	}

	public Dictionary<string, IGameEvent>  GameEvents => gameEvents;
	public IGameEvent GetEvent(string eventID) {
		if (gameEvents.TryGetValue(eventID, out var @event)) {
			return @event;
		}

		return null;
	}
}
