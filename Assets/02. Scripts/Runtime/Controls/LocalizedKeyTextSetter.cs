using System;
using System.Collections;
using System.Collections.Generic;
using Mikrocosmos.Controls;
using Polyglot;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mikrocosmos {
    [Serializable]
    public struct LocalizedControlKey {
        public string mainInputAction;
        public string fallbackInputAction;
    }
    public class LocalizedKeyTextSetter : MonoBehaviour {
        private LocalizedText localizedText;
        private LocalizedTextMesh localizedTextMesh;
        [SerializeField] private List<LocalizedControlKey> inputAction;

        private void Awake() {
            localizedText = GetComponent<LocalizedText>();
            localizedTextMesh = GetComponent<LocalizedTextMesh>();
        }

        private void Start() {
            if (localizedText) {
                foreach (var action in inputAction) {
                    InputAction act = ClientInput.Singleton.PlayerInput.currentActionMap[action.mainInputAction];
                    if (act.GetBindingIndex(ClientInput.Singleton.PlayerInput.currentControlScheme) < 0) {
                        act = ClientInput.Singleton.FindActionInPlayerActionMap(action.fallbackInputAction);
                    }

                    localizedText.AddParameter(ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(act));
                }
            }else if (localizedTextMesh) {
                foreach (var action in inputAction)
                {
                    InputAction act = ClientInput.Singleton.PlayerInput.currentActionMap[action.mainInputAction];
                    if (act.GetBindingIndex(ClientInput.Singleton.PlayerInput.currentControlScheme) < 0)
                    {
                        act = ClientInput.Singleton.FindActionInPlayerActionMap(action.fallbackInputAction);
                    }

                    localizedTextMesh.AddParameter(ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(act));
                }
            }
        }
    }
}
