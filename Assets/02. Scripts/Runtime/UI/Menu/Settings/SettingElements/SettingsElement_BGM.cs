using MikroFramework.AudioKit;
using UnityEngine;

namespace Mikrocosmos
{
	public partial class SettingsElement_BGM : SettingsElement
	{
        [SerializeField] private float defaultValue = 1f;
        public override void OnInit()
        {
            Slider.onValueChanged.AddListener(OnBGMVolumeSliderValueChanged);
        }

        private void OnBGMVolumeSliderValueChanged(float val)
        {
            AudioSystem.Singleton.MusicVolume = val;
            UpdateVolumeText();
        }
        
        public override void OnLoad()
        {

        }



        //auto saved by audio system
        public override void OnSave()
        {
            //ES3.Save<float>(AudioSystem.MusicVolumeStorageKey, AudioSystem.Singleton.MusicVolume);
        }

        public override void OnReset()
        {
            AudioSystem.Singleton.MusicVolume = defaultValue;
            Slider.value = defaultValue;
            UpdateVolumeText();
        }

        //wait audio system initialize
        public override void OnLateLoad()
        {
            float globalVolume = ES3.Load<float>(AudioSystem.MusicVolumeStorageKey, defaultValue);
            Slider.value = globalVolume;
            AudioSystem.Singleton.MusicVolume = globalVolume;
            UpdateVolumeText();
        }

        private void UpdateVolumeText()
        {
            text_Volume.text = (Slider.value * 100).ToString("0") + "%";
        }
    }
}
