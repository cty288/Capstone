using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Mikrocosmos;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractHintCanvas : MonoBehaviour {
    private Transform hintIconSpawnPoint;
    private RectTransform controlLayout;
    private TMP_Text nameText;
    private TMP_Text hintText;
    private Transform camera;

    private void Awake() {
        controlLayout = transform.Find("ControlLayout").GetComponent<RectTransform>();
        hintIconSpawnPoint = transform.Find("ControlLayout/HintIconSpawnPoint");
        hintText = transform.Find("ControlLayout/Text").GetComponent<TMP_Text>();
        nameText = transform.Find("NameText").GetComponent<TMP_Text>();
    }

    private void Start() {
        if (Camera.main) {
            camera = Camera.main.transform;
        }
       
    }

    public void SetHint(InputAction action, string hintText) {
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
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.DOAnchorPos3D(Vector3.zero, 0);
            if (spawnedHint) {
                spawnedHint.transform.SetParent(hintIconSpawnPoint);
            }
        }

        if (this.hintText) {
            this.hintText.text = hintText;
        }

        if (controlLayout) {
            StartCoroutine(RebuildLayout());
        }
    }
    
    public void SetName(string nameText) {
        if (this.nameText) {
            this.nameText.text = nameText;
        }
    }
    
    public void Show() {
        gameObject.SetActive(true);
    }
    
    public void Hide() {
        gameObject.SetActive(false);
    }
    
    private IEnumerator RebuildLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(controlLayout);
        yield return new WaitForEndOfFrame();
        LayoutRebuilder.ForceRebuildLayoutImmediate(controlLayout);
    }

    private void LateUpdate() {
        if (camera) {
            if (Vector3.Distance(transform.position, camera.position) > 0.5f) {
                //lock x and z rotation
                transform.rotation = Quaternion.Euler(0, camera.rotation.eulerAngles.y, 0);
                //transform.rotation = Quaternion.LookRotation(transform.position - camera.position);
                
            }
        }
    }
}
