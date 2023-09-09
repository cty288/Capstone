using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotViewController : MonoBehaviour, IPointerClickHandler, IEndDragHandler, IDragHandler, IBeginDragHandler {
    private TMP_Text numberText;
    private GameObject currentVC = null;
    private Transform spawnPoint;

    private void Awake() {
        numberText = transform.Find("NumberText").GetComponent<TMP_Text>();
        spawnPoint = transform.Find("InventoryItemSpawnPos");
        SetItem(null, 0);
    }

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("OnPointerClick");
    }

    public void OnEndDrag(PointerEventData eventData) {
        //check if pointer is on self
        if (eventData.pointerCurrentRaycast.gameObject != gameObject) {
            Debug.Log("OnEndDrag");
        }
    }
    
    public void Clear() {
        if (currentVC) {
            GameObjectPoolManager.Singleton.Recycle(currentVC);
            numberText.text = "";
        }
        currentVC = null;
    }

    public void SetItem(IResourceEntity topItem, int totalCount) {
        if (currentVC) {
            Clear();
        }
        
        if (topItem == null || totalCount == 0) {
            return;
        }

        string invPrefabName = topItem.InventoryVCPrefabName;
        
        SafeGameObjectPool pool =
            GameObjectPoolManager.Singleton.CreatePoolFromAB(invPrefabName, 
                null, 5, 20, out GameObject prefab);

        currentVC = pool.Allocate();
        IInventoryResourceViewController vc = currentVC.GetComponent<IInventoryResourceViewController>();
        vc.InitWithID(topItem.UUID);
        currentVC.transform.SetParent(spawnPoint);
        currentVC.transform.localPosition = Vector3.zero;
        currentVC.transform.localScale = Vector3.one;
        //set left top right bottom to 10
        RectTransform rectTransform = currentVC.GetComponent<RectTransform>();
        rectTransform.offsetMin = new Vector2(10, 10);
        rectTransform.offsetMax = new Vector2(-10, -10);
        
        numberText.text = totalCount.ToString();

    }
    public void OnDrag(PointerEventData eventData) {
        
    }

    public void OnBeginDrag(PointerEventData eventData) {
        
    }
}
