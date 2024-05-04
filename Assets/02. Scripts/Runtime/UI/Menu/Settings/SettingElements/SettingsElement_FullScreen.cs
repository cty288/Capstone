using UnityEngine;

using UnityEngine.UI;

namespace Mikrocosmos
{
    public partial class SettingsElement_FullScreen : SettingsElement {

        private ToggleGroup toggleGroup;
        private bool isFullScreen = false;

        public override void OnInit() {
            toggleGroup = GetComponent<ToggleGroup>();
            btn_Yes.onValueChanged.AddListener(OnFullScreenStateChanged);
        }

        private void OnFullScreenStateChanged(bool isFullScreen) {
            if (isFullScreen) {
                SetFullScreen(true);
            }
            else {
                SetFullScreen(false);
            }
        }

        public override void OnLoad() {

        }

        public override void OnSave() {
            ES3.Save("fullScreen", isFullScreen);
        }

        private void SetFullScreen(bool isFullScreen) {
            this.isFullScreen = isFullScreen;
            Screen.fullScreen = isFullScreen;
        }

        public override void OnReset() {

        }

        public override void OnLateLoad() {
            isFullScreen = ES3.Load<bool>("fullScreen", false);
            if (isFullScreen) {
                btn_Yes.SetIsOnWithoutNotify(true);
            }
        }
    }
}
