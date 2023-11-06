using System.Collections;
using DG.Tweening;
using Runtime.Controls;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Runtime.GameResources.ViewControllers {
    public class InteractiveHint : MonoBehaviour {
        private Transform hintIconSpawnPoint;
        private RectTransform controlLayout;
        private TMP_Text hintText;

        private void Awake() {
            controlLayout = GetComponent<RectTransform>();
            hintIconSpawnPoint = transform.Find("HintIconSpawnPoint");
            hintText = transform.Find("Text").GetComponent<TMP_Text>();
        }
    

        public void SetHint(InputAction action, string hintText) {
            //destroy all children of hintIconSpawnPoint
        
            if (hintIconSpawnPoint) {
                for (int i = 0; i < hintIconSpawnPoint.childCount; i++) {
                    Destroy(hintIconSpawnPoint.GetChild(i).gameObject);
                }

                hintIconSpawnPoint.gameObject.SetActive(action != null);
                if (action != null) {
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
               
            }

            if (this.hintText) {
                this.hintText.text = hintText;
            }

            if (controlLayout) {
                StartCoroutine(RebuildLayout());
            }
        }
    
    
    
        private IEnumerator RebuildLayout() {
            LayoutRebuilder.ForceRebuildLayoutImmediate(controlLayout);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(controlLayout);
        }
    }
}
