using MikroFramework.AudioKit;
using UnityEngine;


namespace Mikrocosmos
{
	public partial class SettingsElement_MasterVolume : SettingsElement {
        [SerializeField] private float defaultValue = 1f;
        public override void OnInit() {
            Slider.onValueChanged.AddListener(OnMasterVolumeSliderValueChanged);
        }

        private void OnMasterVolumeSliderValueChanged(float val) {
            AudioSystem.Singleton.MasterVolume = val;
            UpdateVolumeText();
        }

        public override void OnLoad() {
            
        }

        

        //auto saved by audio system
        public override void OnSave() {
           // ES3.Save<float>(AudioSystem.MasterVolumeStorageKey, AudioSystem.Singleton.MasterVolume);
        }

        public override void OnReset() {
            AudioSystem.Singleton.MasterVolume = defaultValue;
            Slider.value = defaultValue;
            UpdateVolumeText();
        }

        //wait audio system initialize
        public override void OnLateLoad() {
            float globalVolume = ES3.Load<float>(AudioSystem.MasterVolumeStorageKey, defaultValue);
            Slider.value = globalVolume;
            AudioSystem.Singleton.MasterVolume = globalVolume;
            UpdateVolumeText();
        }

        private void UpdateVolumeText() {
            text_Volume.text = (Slider.value * 100).ToString("0") + "%";
        }

    }
}
