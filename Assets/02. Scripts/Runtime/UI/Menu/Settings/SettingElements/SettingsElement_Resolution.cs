using System;
using System.Collections.Generic;
using UnityEngine;

using TMPro;


namespace Mikrocosmos
{
    [Serializable]
    public struct Resolution {
        [ES3Serializable]
        public int width;
        [ES3Serializable]
        public int height;
    }
    public partial class SettingsElement_Resolution : SettingsElement {
        [SerializeField] private List<Resolution> supportedResolutions = new List<Resolution>();
        private Resolution currentResolution;
        private int currentResolutionIndex;
        public override void OnInit() {
            dropdown_SetResolution.ClearOptions();
            foreach (Resolution resolution in supportedResolutions) {
                dropdown_SetResolution.AddOptions(new List<string> { resolution.width + " x " + resolution.height });
            }
            dropdown_SetResolution.onValueChanged.AddListener(OnDropdownValueChange);
        }

        private void OnDropdownValueChange(int value) {
            SetResolution(value);
        }

        public override void OnLateLoad() {
            currentResolution = ES3.Load<Resolution>("resolution", new Resolution() { width = 1920, height = 1080 });
            currentResolutionIndex = ES3.Load<int>("resolution_index", 0);
            dropdown_SetResolution.SetValueWithoutNotify(currentResolutionIndex);
        }


        private void SetResolution(int index) {
            currentResolution = supportedResolutions[index];
            currentResolutionIndex = index;
            Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
           
        }
        public override void OnLoad() {
           
        }

        public override void OnSave() {
            ES3.Save("resolution", currentResolution);
            ES3.Save("resolution_index", currentResolutionIndex);
        }

        public override void OnReset() {
            //SetResolution(0);
        }
    }
}
