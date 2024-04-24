using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

public class BarrierWall : MonoBehaviour, ICanSendEvent {
    [SerializeField] private string hintLocalizeKey;
    [SerializeField] private float duration = 3f;
    [SerializeField] private string[] keyParameters;

    private void OnCollisionEnter(Collision other) {
        if (string.IsNullOrEmpty(hintLocalizeKey)) {
            return;
        }
        if (other.collider.attachedRigidbody && other.collider.attachedRigidbody.gameObject.CompareTag("Player")) {
            string text = Localization.Get(hintLocalizeKey);
            
            if(keyParameters != null && keyParameters.Length > 0) {
                InputAction[] acts = new InputAction[keyParameters.Length];
                for (int i = 0; i < keyParameters.Length; i++) {
                    acts[i] = ClientInput.Singleton.FindActionInPlayerActionMap(keyParameters[i]);
                }
                object[] localizedKeys = new object[keyParameters.Length];
                for (int i = 0; i < keyParameters.Length; i++) {
                    localizedKeys[i] = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(acts[i]);
                }

                text = Localization.GetFormat(hintLocalizeKey, localizedKeys);
            }
            this.SendEvent<OnShowGameHint>(new OnShowGameHint() {
                duration = duration,
                text = text
            });
            
        }
    }

    public IArchitecture GetArchitecture() {
        return MainGame.Interface;
    }
}
