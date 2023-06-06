using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using UnityEngine;

public struct OnKillCountChanged { //or class, but better to use struct for performance
    public int KillCount;
}

public interface IPlayerModel : IModel {
    void AddKillCount(int count);
    
    int KillCount { get; }
}
public class PlayerModel : AbstractModel, IPlayerModel {
    protected int killCount = 0;

    public int KillCount {
        get {
            return killCount;
        }
    }

    protected override void OnInit() {
        
    }

    public void AddKillCount(int count) {
        this.killCount += count;
        this.SendEvent<OnKillCountChanged>(new OnKillCountChanged(){KillCount = killCount});
    }

    
}
