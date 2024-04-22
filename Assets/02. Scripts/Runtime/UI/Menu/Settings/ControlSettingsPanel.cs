using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mikrocosmos
{
	public partial class ControlSettingsPanel : SettingsSubPanel {
        
        protected override void OnPanelInit() {
            
        }

        /*private void OnToggleValueChanged(bool isOn) {
            if (isOn) {
                OpenSubPanel(ToggleTabs.GetFirstActiveToggle().transform.GetSiblingIndex());
            }
        }
        */

        private void OpenSubPanel(int index) {
            for (int i = 0; i < SubPanels.transform.childCount; i++) {
                GameObject panel = SubPanels.transform.GetChild(i).gameObject;
                panel.SetActive(i == index);
            }
        }
        protected override void OnPanelOpen() {
            //toggle_Keyboard.SetIsOnWithoutNotify(true);
            OpenSubPanel(0);
        }

        protected override void OnPanelClosed() {
            
        }

        [SerializeField] protected InputActionAsset inputActions;
        protected override void OnDisable() {
            base.OnDisable();
            RebindSaveLoad.Save(inputActions);
        }

        private void OnApplicationQuit() {
            RebindSaveLoad.Save(inputActions);
        }
    }
}
