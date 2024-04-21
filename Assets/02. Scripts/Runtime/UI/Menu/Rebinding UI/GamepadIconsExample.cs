using System;
using DG.Tweening;
using Mikrocosmos;
using MikroFramework.ResKit;
using Runtime.Controls;
using UnityEngine.UI;

////TODO: have updateBindingUIEvent receive a control path string, too (in addition to the device layout name)

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    /// <summary>
    /// This is an example for how to override the default display behavior of bindings. The component
    /// hooks into <see cref="RebindActionUI.updateBindingUIEvent"/> which is triggered when UI display
    /// of a binding should be refreshed. It then checks whether we have an icon for the current binding
    /// and if so, replaces the default text display with an icon.
    /// </summary>
    public class GamepadIconsExample : MonoBehaviour
    {
        public GamepadBindingData xbox;
        public GamepadBindingData ps4;
        public KeyboardMouseBindingData keyboardMouse;

        private ResLoader resLoader;
        private BindingKeyData _bindingKeyData;

        private void Awake() {
           
            ResLoader.Create(loader => {
                resLoader = loader;
            });
            
            _bindingKeyData = resLoader.LoadSync<BindingKeyData>("data", "BindingData");
        }

        protected void OnEnable()
        {
            // Hook into all updateBindingUIEvents on all RebindActionUI components in our hierarchy.
            var rebindUIComponents = transform.GetComponentsInChildren<RebindActionUI>(true);
            foreach (var component in rebindUIComponents)
            {
                component.updateBindingUIEvent.AddListener(OnUpdateBindingDisplay);
                component.UpdateBindingDisplay();
            }

           
        }

        protected void OnUpdateBindingDisplay(RebindActionUI component, string bindingDisplayString, string deviceLayoutName, string controlPath)
        {
            if (resLoader == null) {
                ResLoader.Create(loader => {
                    resLoader = loader;
                });
                _bindingKeyData = resLoader.LoadSync<BindingKeyData>("data", "BindingData");
            }
            if (string.IsNullOrEmpty(deviceLayoutName) || string.IsNullOrEmpty(controlPath))
                return;

            var icon = default(Sprite);
            BindingInfo info = _bindingKeyData.GetBindingInfoFromDeviceNameAndControlPath(deviceLayoutName, controlPath);
            /*
            
            if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
                icon = ps4.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
                icon = xbox.GetSprite(controlPath);

            if (deviceLayoutName == "Keyboard" || deviceLayoutName == "Mouse")
                icon = keyboardMouse.GetSprite(controlPath);*/
            if (info != null) {
                icon = info.Sprite;
            }

            var textComponent = component.bindingText;

            // Grab Image component.
            var imageGO = textComponent.transform.parent.Find("ActionBindingIcon");
            var imageComponent = imageGO.GetComponent<Image>();

            if (icon != null)
            {
                textComponent.DOFade(0,0); // .SetActive(false);
                imageComponent.sprite = icon;
                imageComponent.DOFade(1, 0);// gameObject.SetActive(true);
            }
            else {
                textComponent.DOFade(1, 0);// gameObject.SetActive(true);
                imageComponent.DOFade(0, 0);// gameObject.SetActive(false);
            }
        }

        
    }
}
