using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.UIKit;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public abstract class SettingsSubPanel : MikroUIPanel {
        protected SettingsElement[] settingsElements = Array.Empty<SettingsElement>();
        [SerializeField] protected Button resetButton;
        public override void OnInit() {
            settingsElements = GetComponentsInChildren<SettingsElement>(true);
            foreach (SettingsElement element in settingsElements) {
                element.OnInit();
            }
            if (resetButton) {
                resetButton.onClick.AddListener(OnResetButtonClicked);
            }
            OnPanelInit();
        }

        protected abstract void OnPanelInit();

        private void OnResetButtonClicked() {
            foreach (SettingsElement settingsElement in settingsElements) {
                settingsElement.OnReset();
                settingsElement.OnSave();
            }
        }

        public override void OnOpen(UIMsg msg) {
            foreach (SettingsElement settingsElement in settingsElements) {
                settingsElement.OnLoad();
                StartCoroutine(LateLoad(settingsElement));
            }
            OnPanelOpen();
        }

        private IEnumerator LateLoad(SettingsElement element) {
            yield return null;
            element.OnLateLoad();
        }
        protected abstract void OnPanelOpen();
        public override void OnClosed() {
            OnPanelClosed();
        }

        protected abstract void OnPanelClosed();
        
        protected virtual void OnDisable() {
            foreach (SettingsElement settingsElement in settingsElements) {
                settingsElement.OnSave();
            }
        }
    }
}
