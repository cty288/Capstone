using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using Framework;
using NUnit.Framework;
using Runtime.Enemies.Model;
using UnityEngine;

public class TestTimeSystem
{
	private IGameTimeModel gameTimeModel;
	private int newDayStartHour = 5;
    [SetUp]
    public void SetUp() {
	    ((MainGame_Test) MainGame_Test.Interface).ClearSave();
	    Reset();
    }
    
    private void Reset() {
		((MainGame_Test) MainGame_Test.Interface).Reset();
		gameTimeModel = MainGame_Test.Interface.GetModel<IGameTimeModel>();
		newDayStartHour = GameTimeModel.NewDayStartHour;
    }



    [Test]
    public void TestBasicTimeSystem() {
       //test when the game starts
      // Assert.IsTrue(gameTimeModel.DayCountThisRound.Value == 1);
       Assert.IsTrue(gameTimeModel.GlobalTime.Value == gameTimeModel.GetRoundStartTime());
       Assert.IsTrue(gameTimeModel.GetDayCountFromGlobalTime(gameTimeModel.GlobalTime.Value) == 1);
       DateTime globalTime = gameTimeModel.GetGlobalTimeFromDayCount(1);
       Assert.IsTrue(globalTime.Day == gameTimeModel.GlobalTime.Value.Day);
       Assert.IsTrue(globalTime.Hour == newDayStartHour);
       
       DateTime globalTime2 = gameTimeModel.GetGlobalTimeFromNow(1, 1, 1, 1);
       Assert.IsTrue(globalTime2.Day == gameTimeModel.GlobalTime.Value.Day + 1);
       Assert.IsTrue(globalTime2.Hour == newDayStartHour + 1);
       Assert.IsTrue(globalTime2.Minute == 1);
    }

    [Test]
    public void TestTimePass() {
	    gameTimeModel.NextMinute();
	    Assert.IsTrue(gameTimeModel.GlobalTime.Value == new DateTime(2024, 1, 1, newDayStartHour, 1, 0));
	    gameTimeModel.NextMinute();
	    Assert.IsTrue(gameTimeModel.GlobalTime.Value == new DateTime(2024, 1, 1, newDayStartHour, 2, 0));
	    
	    gameTimeModel.NextDay();
	    Assert.IsTrue(gameTimeModel.GlobalTime.Value == new DateTime(2024, 1, 2, newDayStartHour, 0, 0));
	    
	    int minutesLeaped = gameTimeModel.NextDay();
	    Assert.IsTrue(gameTimeModel.GlobalTime.Value == new DateTime(2024, 1, 3, newDayStartHour, 0, 0));
	   // Assert.IsTrue(gameTimeModel.DayCountThisRound.Value ==  3);
	    Assert.IsTrue(minutesLeaped == 1440);

	    int minutesLeaped2 =  gameTimeModel.NewRound();
	    Assert.IsTrue(gameTimeModel.GlobalTime.Value == new DateTime(2024, 1, 4, newDayStartHour, 0, 0));
	   // Assert.IsTrue(gameTimeModel.DayCountThisRound.Value == 1);
	    Assert.IsTrue(minutesLeaped2 == 1440);
    }

    [Test]
    public void TestTimeCalculation() {
	    DateTime globalTime = gameTimeModel.GetGlobalTimeFromNow(5, 3, 3, 30);
	    Assert.IsTrue(globalTime == new DateTime(2024, 1, 6, newDayStartHour + 3, 3, 30));
	    
	    DateTime globalTime2 = new DateTime(2024, 1, 6, newDayStartHour + 3, 3, 30);
	    Assert.IsTrue(gameTimeModel.GetDayCountFromGlobalTime(globalTime2) == 6);
	    
	    DateTime globalTime3 = new DateTime(2024, 1, 6, newDayStartHour - 3, 3, 30);
	    Assert.IsTrue(gameTimeModel.GetDayCountFromGlobalTime(globalTime3) == 5);
    }
}
