using Framework;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Image = UnityEngine.UI.Image;

namespace Runtime.Controls
{
    public class ControlInfoFactory : MikroSingleton<ControlInfoFactory> {
        private BindingKeyData bindingKeyData;
        private ResLoader resLoader;
        private ControlInfoFactory() {
            resLoader = MainGame.Interface.GetUtility<ResLoader>();
            bindingKeyData = resLoader
                .LoadSync<BindingKeyData>("data", "BindingData");
        }
      
        public string GetBindingKeyLocalizedName(InputAction action) {
            if (action == null) {
                return "";
            }

           
            BindingInfo info = bindingKeyData.GetBindingInfo(action, out string internalDisplayName,
                ClientInput.Singleton.PlayerInput.currentControlScheme);
            if (string.IsNullOrEmpty(info.LocalizationKey)) {
                return internalDisplayName;
            }

            return Localization.Get(info.LocalizationKey);
        }
        
        public GameObject GetBindingKeyGameObject(InputAction action, out BindingInfo info, out string internalDisplayName) {
            if (action == null) {
                internalDisplayName = "";
                info = null;
                return null;
            }
            info = bindingKeyData.GetBindingInfo(action, out internalDisplayName,
                ClientInput.Singleton.PlayerInput.currentControlScheme);

            string controlPath = info.ControlPath;

            switch (controlPath) {
                case "leftButton":
                    return GameObject.Instantiate(resLoader.LoadSync<GameObject>("description", "LeftClick"));
                case "rightButton":
                    return GameObject.Instantiate(resLoader.LoadSync<GameObject>("description", "RightClick"));
                default:
                    return GetGeneralBindingKeyGameObject(info, action);
            }
        }

        private GameObject GetGeneralBindingKeyGameObject(BindingInfo info, InputAction action) {
            if (action == null) {
                return null;
            }
            if (info.Sprite) {
                GameObject generalBindingKey =
                    GameObject.Instantiate(resLoader.LoadSync<GameObject>("description", "GeneralHintIcon"));
                generalBindingKey.GetComponent<Image>().sprite = info.Sprite;
                return generalBindingKey;
            } else {
                string localizedName = GetBindingKeyLocalizedName(action);
                if (string.IsNullOrEmpty(localizedName)) {
                    return null;
                }
                GameObject bindingKeyNoSprite =
                    GameObject.Instantiate(resLoader.LoadSync<GameObject>("description", "GeneralHintIconNoSprite"));
                bindingKeyNoSprite.GetComponentInChildren<TMP_Text>().text = localizedName;
                return bindingKeyNoSprite;
            }
        }
    }
}
