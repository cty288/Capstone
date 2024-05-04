using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintMessageTrigger : MonoBehaviour {
   [SerializeField] private HintMessageGroup[] messageGroup;
   [SerializeField] private bool canTriggerMultipleTimes = false;

   private Collider collider;
   private int currentMessageGroupIndex = 0;
   private bool hasTriggered = false;

   private void Awake() {
      HintManager.Singleton.RegisterOnPanelClose(OnPanelClose);
      collider = GetComponent<Collider>();
   }

   private void OnPanelClose(HintPanel panel) {
      if (panel == null) {
         return;
      }
      
      if (currentMessageGroupIndex < messageGroup?.Length && panel.CurrentMessageGroup == messageGroup[currentMessageGroupIndex]) {
         currentMessageGroupIndex++;
         if (currentMessageGroupIndex < messageGroup.Length) {
            HintManager.Singleton.ShowHint(messageGroup[currentMessageGroupIndex]);
         }
      }
   }

   private void OnDestroy() {
      HintManager.Singleton.UnregisterOnPanelClose(OnPanelClose);
   }

   private void OnTriggerEnter(Collider other) {
      if (!collider.enabled) {
         return;
      }
      if (!other.gameObject.CompareTag("Player")) {
         return;
      }
      if (hasTriggered && !canTriggerMultipleTimes) {
         return;
      }

      currentMessageGroupIndex = 0;
      HintManager.Singleton.ShowHint(messageGroup[0]);
      hasTriggered = true;
   }
}
