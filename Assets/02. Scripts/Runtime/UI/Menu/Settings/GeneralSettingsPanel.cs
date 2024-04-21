using System;
using MikroFramework.UIKit;
using UnityEngine;


namespace Mikrocosmos
{
	public partial class GeneralSettingsPanel : SettingsSubPanel{
        protected override void OnPanelInit() {
           
        }

        private void Awake() {
            Btn_About.onClick.AddListener(OnAboutPanelClicked);
        }

        private void OnAboutPanelClicked() {
            UIManager.Singleton.Open<AboutPanel>(Parent, null, false);
        }

        protected override void OnPanelOpen() {
          
        }

        protected override void OnPanelClosed() {
            
        }
    }
}
