using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.AudioKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mikrocosmos
{
    [RequireComponent(typeof(Button))]
    public class ButtonAudio : MonoBehaviour, ISubmitHandler, IPointerClickHandler {
        [SerializeField] private AudioClip interactiveClickSound;
        [SerializeField] private AudioClip deactiveClickSound;

        private Button button;

        private void Awake() {
            button = GetComponent<Button>();
        }


        public void OnPointerClick(PointerEventData eventData) {
            
            if (button.interactable)
            {
                if (interactiveClickSound) {
                    AudioSystem.Singleton.Play2DSound(interactiveClickSound, 2f);
                }
                
            }
            else {
                if (deactiveClickSound) {
                    AudioSystem.Singleton.Play2DSound(deactiveClickSound, 2f);
                }
               
            }
        }

        public void OnSubmit(BaseEventData eventData) {
            if (button.interactable)
            {
                if (interactiveClickSound) {
                    AudioSystem.Singleton.Play2DSound(interactiveClickSound, 2f);
                }
               
            }
            else
            {
                if (deactiveClickSound) {
                    AudioSystem.Singleton.Play2DSound(deactiveClickSound, 2f);
                }
               
            }
        }
    }
}
