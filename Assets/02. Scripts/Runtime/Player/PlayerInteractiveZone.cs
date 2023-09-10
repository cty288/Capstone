using System.Collections.Generic;
using MikroFramework.Utilities;
using Runtime.DataFramework.ViewControllers.Entities;
using UnityEngine;

namespace Runtime.Player {
    public class PlayerInteractiveZone : MonoBehaviour {
        private TriggerCheck interactiveHintTriggerCheck;
        private HashSet<IEntityViewController> entityViewControllers = new HashSet<IEntityViewController>();

        private void Awake() {
            interactiveHintTriggerCheck = GetComponent<TriggerCheck>();
            interactiveHintTriggerCheck.OnEnter += OnEnterInteractiveCheck;
            interactiveHintTriggerCheck.OnExit += OnExitInteractiveCheck;
        }

        private void OnExitInteractiveCheck(Collider other) {
            if(other.TryGetComponent<IEntityViewController>(out var entityViewController)) {
                entityViewController.OnPlayerExitInteractiveZone(transform.parent.gameObject, this);
                entityViewControllers.Remove(entityViewController);
            }
        }

        private void OnEnterInteractiveCheck(Collider other) {
            if(other.TryGetComponent<IEntityViewController>(out var entityViewController)) {
                entityViewController.OnPlayerEnterInteractiveZone(transform.parent.gameObject, this);
                entityViewControllers.Add(entityViewController);
            }
        }
    
        public bool IsInZone(IEntityViewController entityViewController) {
            return entityViewControllers.Contains(entityViewController);
        }

        private void OnDestroy() {
            interactiveHintTriggerCheck.OnEnter -= OnEnterInteractiveCheck;
            interactiveHintTriggerCheck.OnExit -= OnExitInteractiveCheck;
        }
    }
}
