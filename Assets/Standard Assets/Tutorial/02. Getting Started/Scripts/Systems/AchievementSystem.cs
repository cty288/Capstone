using MikroFramework.Architecture;

public interface IAchievementSystem : ISystem {
    
}

public struct OnGetAchievement {
	public string AchievementName;
	
	public OnGetAchievement(string achievementName) {
		AchievementName = achievementName;
	}
}

public class AchievementSystem : AbstractSystem, IAchievementSystem {
	
	
	protected override void OnInit() {
		this.RegisterEvent<OnKillCountChanged>(OnKillCountChanged);
	}

	private void OnKillCountChanged(OnKillCountChanged e) {
		if (e.KillCount >= 3) {
			this.SendEvent<OnGetAchievement>(new OnGetAchievement("Enemy Killer"));
		}
	}
}
