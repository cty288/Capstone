using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using UnityEngine;

public class SkillUpgradePanel : AbstractPanel, IController, IGameUIPanel 
{

    public override void OnInit() {
       
    }

    public override void OnOpen(UIMsg msg) {
       
    }

    public override void OnClosed() {
      
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }

    public IPanel GetClosePanel() {
        return this;
    }
}
