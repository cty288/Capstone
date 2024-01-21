using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.GameResources;
using TMPro;
using UnityEngine;

public class ResearchNotificationPanel : AbstractMikroController<MainGame> {
    [SerializeField]
    private TMP_Text notificationText;

    


    public void SetContent(string[] entityNames, string npcName) {
        string[] localizedEntityNames = new string[entityNames.Length];
        
        for (int i = 0; i < entityNames.Length; i++) {
            var info = ResourceTemplates.Singleton.GetResourceTemplates(entityNames[i]);
            string displayName = info.TemplateEntity.GetDisplayName();
            localizedEntityNames[i] = "- " + displayName;
        }

        string joinedNames = string.Join("\n", localizedEntityNames);
        notificationText.text = Localization.GetFormat("RESEARCH_NOT_BODY", joinedNames, npcName);
    }
}
