using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using UnityEngine;


public struct OnNewDayStart {
	public int DayCount;
}

public interface IGameTimeModel : ISavableModel {
     //public BindableProperty<int> DayCountThisRound { get; } //the number of days since the player start out from the base
     public BindableProperty<DateTime> GlobalTime { get; } //the global time in the game, this will always increase for the save file
     
     void NextMinute(); //increase the global time by one minute
     
     
     /// <summary>
     /// For example, if dayCount param = 3, and DayCountThisRound = 1,
     /// and the base time is 2024/1/1 5:00:00,
     /// then the global time will be 2 days after the start round time, which is 2024/1/3 5:00:00
     /// This will always return a time with hour = NewDayStartHour, minute = 0, second = 0
     /// </summary>
     /// <param name="dayCount"></param>
     /// <returns></returns>
     DateTime GetGlobalTimeFromDayCount(int dayCount); //get the global time from the day count
     
     /// <summary>
     /// get the global time from the day count and the time of the day
     /// </summary>
     /// <param name="dayCount"></param>
     /// <param name="hour"></param>
     /// <param name="minute"></param>
     /// <param name="second"></param>
     /// <returns></returns>
     DateTime GetGlobalTimeFromNow(int day, int hour, int minute, int second);
     
     
     /// <summary>
     /// get the day count from the global time. Note that this is not equivalent to GlobalTime.Day,
     /// as a new day in our game starts at NewDayStartHour, which is 5:00:00 by default
     /// </summary>
     /// <param name="globalTime"></param>
     /// <returns></returns>
     int GetDayCountFromGlobalTime(DateTime globalTime);
     
     /// <summary>
     /// get the start time, which is the time when the player start out from the base this round
     /// </summary>
     /// <returns></returns>
     DateTime GetRoundStartTime(); 

     /// <summary>
     /// jump to next day, return minutes leaped
     /// </summary>
     /// <returns></returns>
     int NextDay(); 

     /// <summary>
     /// start a new round, return minutes leaped; this will skip to the next day and reset the day count
     /// </summary>
     /// <returns></returns>
     int NewRound(); 
}


public class GameTimeModel : AbstractSavableModel, IGameTimeModel {
    
    public const float DayLength = 420f; // day length in seconds
    public const int NewDayStartHour = 5;
    public const int NightStartHour = 20;
    
    [field: ES3Serializable]
    public BindableProperty<int> DayCountThisRound { get; } = new BindableProperty<int>(1);
    
    [field: ES3Serializable] 
    public BindableProperty<DateTime> GlobalTime { get; } = new BindableProperty<DateTime>();
    
    [field: ES3Serializable]
    private DateTime RoundStartTime { get; set; }
    
    
    public void NextMinute() {
	    GlobalTime.Value = GlobalTime.Value.AddMinutes(1);
	    if (GlobalTime.Value.Hour == NewDayStartHour && GlobalTime.Value.Minute == 0) {
		    DayCountThisRound.Value++;
		    this.SendEvent<OnNewDayStart>(new OnNewDayStart() {
			    DayCount =  DayCountThisRound.Value
		    });
	    }
    }

    public DateTime GetGlobalTimeFromDayCount(int dayCount) {
	    return RoundStartTime.AddDays(dayCount - 1);
    }

    public DateTime GetGlobalTimeFromNow(int day, int hour, int minute, int second) {
	    return RoundStartTime.AddDays(day).AddHours(hour).AddMinutes(minute).AddSeconds(second);
    }

    public int GetDayCountFromGlobalTime(DateTime globalTime) {
	    // get the day count from the global time. Note that this is not equivalent to GlobalTime.Day,
	    // as a new day in our game starts at NewDayStartHour, which is 5:00:00 by default
	    return (int) (globalTime - RoundStartTime).TotalDays + 1; 
    }

    public DateTime GetRoundStartTime() {
	    return RoundStartTime;
    }

    public int NextDay() {
	    var nextDay = GetGlobalTimeFromDayCount(DayCountThisRound.Value + 1);
	    var minutesLeaped = (int) (nextDay - GlobalTime.Value).TotalMinutes;
	    GlobalTime.Value = nextDay;
	    DayCountThisRound.Value++;
	    this.SendEvent<OnNewDayStart>(new OnNewDayStart() {
		    DayCount = DayCountThisRound.Value
	    });
	    return minutesLeaped;
    }

    public int NewRound() {
	    DateTime nextDay = GetGlobalTimeFromDayCount(DayCountThisRound.Value + 1);
	    var minutesLeaped = (int) (nextDay - GlobalTime.Value).TotalMinutes;
	    GlobalTime.Value = nextDay;
	    DayCountThisRound.Value = 1;
	    RoundStartTime = nextDay;
	    this.SendEvent<OnNewDayStart>(new OnNewDayStart() {
		    DayCount = DayCountThisRound.Value
	    });
	    return minutesLeaped;
    }

    protected override void OnInit() {
	    base.OnInit();
	    if (IsFirstTimeCreated) {
		    GlobalTime.Value = new DateTime(2024, 1, 1, NewDayStartHour, 0, 0);
		    RoundStartTime = GlobalTime.Value;
	    }
    }
}
