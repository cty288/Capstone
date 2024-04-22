using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ResetAllBindings : MonoBehaviour {
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private List<string> devices = new List<string>();
    public void ResetAllCustomBindings() {
        if (devices.Count == 0) {
            foreach (InputActionMap actionMap in inputActions.actionMaps) {
                actionMap.RemoveAllBindingOverrides();
            }
        }
        else {
            foreach (InputActionMap inputActionMap in inputActions.actionMaps) {
                var allActions = inputActionMap.actions;
                foreach (InputAction action in allActions) {
                    var allBindings = action.bindings;
                    for (int i = 0; i < allBindings.Count; i++) {
                        InputBinding binding = allBindings[i];


                        foreach (string device in devices) {
                            if (binding.effectivePath.StartsWith(device)) {
                                action.RemoveBindingOverride(i);
                                break;
                            }
                        }
                    }
                }
            }
        }

        RebindSaveLoad.Save(inputActions);
        
    }
}
