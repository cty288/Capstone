using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using UnityEngine;

public enum EventElapseType {
	Predetermined,
	ExcludeTimeLeap
}
public interface IGameEvent : IPoolable {
	public string EventID { get; set; }
	public int RemainingMinutesToTrigger { get; set; }
	
	public int TotalMinutes { get; set; }
	
	public int StartWithLevel { get; set; }
	public EventElapseType ElapseType { get; }
	
	public void OnInitialize();
	
	public void OnEventTriggered();
	/// <summary>
	/// Happens when the event is leaped due to time jumping. Most likely to be called when
	/// switching between levels and CanPersistToOtherLevels is false
	/// </summary>
	public void OnEventLeaped();
	//TODO: question: leaped and new level happen at the same time? -> no special consideration needed, test it
	
	/// <summary>
	/// Can the event persist to other levels? If not, if the event is not triggered in this level, it will be removed
	/// and OnEventLeaped() will be called
	/// </summary>
	public bool CanPersistToOtherLevels { get; }
}

public abstract class GameEvent<T> : IGameEvent, ICanSendEvent, ICanGetUtility, ICanGetSystem, ICanRegisterEvent, IPoolable
	where T : GameEvent<T>, new(){
	
	[field: ES3Serializable]
	public string EventID { get; set; }

	[field: ES3Serializable]
	public int RemainingMinutesToTrigger { get; set; }

	[field: ES3Serializable]
	public int TotalMinutes { get; set; }


	[field: ES3Serializable]

	public int StartWithLevel { get; set; }


	public abstract EventElapseType ElapseType { get; }
	
	public void OnInitialize() {
		OnInitialized();
	}

	public void OnEventTriggered() {
		OnTriggered();
	}

	public void OnEventLeaped() {
		OnLeaped();
	}
	
	public abstract void OnInitialized();
	public abstract void OnTriggered();
	public abstract void OnLeaped();

	public abstract bool CanPersistToOtherLevels { get; }
	public IArchitecture GetArchitecture() {
		return MainGame.Interface;
	}
	
	public static T Allocate() {
		return SafeObjectPool<T>.Singleton.Allocate();
	}

	public void OnRecycled() {
		OnEventRecycled();
		RemainingMinutesToTrigger = 0;
		StartWithLevel = 0;
		TotalMinutes = 0;
	}
	
	public abstract void OnEventRecycled();

	public bool IsRecycled { get; set; }
	public void RecycleToCache() {
		SafeObjectPool<T>.Singleton.Recycle(this as T);
	}
}
