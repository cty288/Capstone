using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mikrocosmos;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractHintCanvas : MonoBehaviour {
    private Transform hintIconSpawnPoint;
    private TMP_Text nameText;
    
    
    
    public void SetHint(InputAction action, string name) {
        //destroy all children of hintIconSpawnPoint
        if (hintIconSpawnPoint) {
            for (int i = 0; i < hintIconSpawnPoint.childCount; i++) {
                Destroy(hintIconSpawnPoint.GetChild(i).gameObject);
            }

            GameObject spawnedHint = ControlInfoFactory.Singleton.GetBindingKeyGameObject(action, out BindingInfo info,
                out string internalDisplayName);
            
            spawnedHint.transform.SetParent(hintIconSpawnPoint);
            RectTransform rectTransform = spawnedHint.GetComponent<RectTransform>();
            rectTransform.anchoredPosition3D = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            rectTransform.DOAnchorPos3D(Vector3.zero, 0);
            if (spawnedHint) {
                spawnedHint.transform.SetParent(hintIconSpawnPoint);
            }
        }
    }
    
    
}
