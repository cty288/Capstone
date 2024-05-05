using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using TMPro;
using UnityEngine;

public class CanvasWithQuery : AbstractMikroController<KillGame>
{
    private TMP_Text killCountText;
    private GameObject achievementPanel;
	
    private void Awake() {
        killCountText = transform.Find("KillCountText").GetComponent<TMP_Text>();
        achievementPanel = transform.Find("AchievementUnlockPanel").gameObject;
        achievementPanel.SetActive(false); //disable it first
        killCountText.text = "Kill Count: -1"; //I change the text to -1 to make sure the initialization is working

        //int killCount = this.GetModel<IPlayerModel>().KillCount;
        int killCount = this.SendQuery<int>(new KillCountQuery());
        killCountText.text = $"Kill Count: {killCount}";
        
        this.RegisterEvent<OnKillCountChanged>(OnKillCountChanged).
            UnRegisterWhenGameObjectDestroyed(gameObject); //auto unregister the event when the gameobject is destroyed
		
        this.RegisterEvent<OnGetAchievement>(OnGetAchievement).UnRegisterWhenGameObjectDestroyed(gameObject);
    }

    private void OnGetAchievement(OnGetAchievement e) {
        achievementPanel.SetActive(true);
        //in real development it's better to give achievement panel a script and call that script to change its displayed name, but i'm lazy
        achievementPanel.transform.Find("Achievement").GetComponent<TMP_Text>().text = e.AchievementName;
		
        this.Delay(1f, () => { 
            achievementPanel.SetActive(false); //disable it after 1 second
        });
    }

    private void OnKillCountChanged(OnKillCountChanged e) {
        killCountText.text = $"Kill Count: {e.KillCount}";
    }
}
