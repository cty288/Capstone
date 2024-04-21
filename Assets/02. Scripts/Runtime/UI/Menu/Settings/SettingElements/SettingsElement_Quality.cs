using System.Linq;
using UnityEngine;

using UnityEngine.UI;


namespace Mikrocosmos
{
	public partial class SettingsElement_Quality : SettingsElement {
        private ToggleGroup toggleGroup;
        private Toggle[] toggles;

        private int currentQualityLevel;
        public override void OnInit() {
            toggleGroup = GetComponent<ToggleGroup>();
            toggles = GetComponentsInChildren<Toggle>(true);
            foreach (var toggle in toggles) {
                toggle.onValueChanged.AddListener(OnToggleValueChange);
            }
        }

        private void OnToggleValueChange(bool isOn) {
            Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
            SetQuality(toggle.transform.GetSiblingIndex());
        }

        public override void OnLoad()
        {

        }

        public override void OnSave()
        {
            ES3.Save("quality_level", currentQualityLevel);
        }

        private void SetQuality(int index)
        {
            this.currentQualityLevel = index;
            QualitySettings.SetQualityLevel(index);
            Debug.Log($"Current Quality: {QualitySettings.GetQualityLevel()}");
        }
        public override void OnReset() {
            SetQuality(2);
            toggles[currentQualityLevel].SetIsOnWithoutNotify(true);
        }

        public override void OnLateLoad() {
            currentQualityLevel = ES3.Load<int>("quality_level", 2);
            toggles[currentQualityLevel].SetIsOnWithoutNotify(true);
        }
    }
}
