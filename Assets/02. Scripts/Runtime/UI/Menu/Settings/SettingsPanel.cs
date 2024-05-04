using System;
using System.Collections.Generic;
using System.Linq;
using MikroFramework.UIKit;
using UnityEngine;

using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Mikrocosmos
{
	public partial class SettingsPanel : AbstractPanelContainer {
        private IPanel activePanel = null;
        private List<IPanel> allSubPanels = new List<IPanel>();
        public override void OnInit() {
            allSubPanels = GetComponentsInChildren<SettingsSubPanel>().Select((panel => panel.GetComponent<IPanel>()))
                .ToList();
            
            btn_General.onValueChanged.AddListener(OnGeneralOpen);
            btn_Controls.onValueChanged.AddListener(OnControlsOpen);
            btn_Graphics.onValueChanged.AddListener(OnGraphicsOpen);

           
        }

        private void OnGraphicsOpen(bool open) {
            OpenSubPanel<GraphicsSettingsPanel>();
        }


        private void OnControlsOpen(bool open) {
            OpenSubPanel<ControlSettingsPanel>();
        }

        private void OnGeneralOpen(bool open) {
            OpenSubPanel<GeneralSettingsPanel>();
        }

        private void OpenSubPanel<T>() where T:class, IPanel {
            if (activePanel != null) {
                if (activePanel.GetType() != typeof(T)) {
                    UIManager.Singleton.ClosePanel(activePanel);
                }
            }
            if (activePanel==null || activePanel.GetType() != typeof(T)) {
                activePanel = UIManager.Singleton.Open<T>(this, null);
            }
        }

        
        private void OnTopBarButtonClicked() {
         
        }

       
        public override void OnOpen(UIMsg msg) {
            OpenSubPanel<GeneralSettingsPanel>();
            btn_General.SetIsOnWithoutNotify(true);
        }

        public override void OnClosed() {
            activePanel = null;
        }
    }
}
