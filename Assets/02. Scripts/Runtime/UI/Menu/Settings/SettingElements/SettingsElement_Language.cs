using System.Collections;
using System.Collections.Generic;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Mikrocosmos
{
    public class SettingsElement_Language : SettingsElement {
        private Dropdown dropdown;
        public override void OnInit() {
            dropdown = GetComponentInChildren<Dropdown>(true);
            dropdown.onValueChanged.AddListener(OnLanguageChanged);
        }

        private void OnLanguageChanged(int index) {
            ES3.Save("language", Localization.Instance.SupportedLanguages[index]);
        }

        public override void OnLoad() {
           
        }

        public override void OnSave() {
            
        }

        public override void OnReset() {
            
        }

        public override void OnLateLoad() {
           
        }
    }
}
