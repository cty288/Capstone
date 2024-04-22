using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;
using MikroFramework.Singletons;
using MikroFramework.UIKit;
using Runtime.UI;
using UnityEngine;

public class HintManager : MikroSingleton<HintManager>, ISingleton {
    private HintManager() {
    }

    private Action<HintPanel> onPanelClose;
    public void ShowHint(HintMessageGroup messageGroup) {
        HintPanel panel = null;
        switch (messageGroup.messageType) {
            case HintMessageType.NPCDialogue:
                panel = UIManager.Singleton.GetPanel<NPCDialoguePanel>(true);
                if (panel == null) {
                    panel = MainUI.Singleton.OpenOrGetClose<NPCDialoguePanel>(null, null, false);
                }
                break;
            case HintMessageType.Panel:
                panel = UIManager.Singleton.GetPanel<PanelHint>(true);
                if (panel == null) {
                    panel = MainUI.Singleton.OpenOrGetClose<PanelHint>(null, null, false);
                }
                break;
            case HintMessageType.Subtitle:
                panel = UIManager.Singleton.GetPanel<SubtitleHint>(true);
                if (panel == null) {
                    panel = MainUI.Singleton.OpenOrGetClose<SubtitleHint>(null, null, true, false);
                }
                break;
        }

        if (panel != null) {
            panel.RegisterOnPanelClose(OnPanelClose);
            panel.StartNewMessageGroup(messageGroup);
           
        }
    }

    private void OnPanelClose(HintPanel panel) {
        onPanelClose?.Invoke(panel);
    }
    
    public void RegisterOnPanelClose(Action<HintPanel> action) {
        onPanelClose += action;
    }
    
    public void UnregisterOnPanelClose(Action<HintPanel> action) {
        onPanelClose -= action;
    }
}
