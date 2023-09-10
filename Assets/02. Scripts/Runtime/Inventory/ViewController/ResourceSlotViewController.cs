using System;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Pool;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Commands;
using Runtime.Inventory.Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Runtime.Inventory.ViewController {
    public class ResourceSlotViewController : AbstractMikroController<MainGame>, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
        IEndDragHandler, IDragHandler, IBeginDragHandler {
        private TMP_Text numberText;
        private GameObject topVC = null;
        private Transform spawnPoint;
        private ResourceSlot slot;
        private Vector2 dragStartPos;
        private bool startDragTriggered = false;
        private Button button;
        private Image slotHoverBG;

        public static GameObject pointerDownObject = null;
        private void Awake() {
            numberText = transform.Find("InventoryItemSpawnPos/NumberText").GetComponent<TMP_Text>();
            spawnPoint = transform.Find("InventoryItemSpawnPos");
            slotHoverBG = transform.Find("SlotHoverBG").GetComponent<Image>();
            button = GetComponent<Button>();
            Clear();
        }

        public void OnPointerClick() {
           
        }
        
        public void OnPointerDown(PointerEventData eventData) {
            pointerDownObject = gameObject;
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (topVC && !startDragTriggered && pointerDownObject == gameObject) {
                OnPointerClick();
            }
        }
        
        
        public void OnDrag(PointerEventData eventData) {
            if (topVC && Vector2.Distance(eventData.position, dragStartPos) > 10) {
                if (!startDragTriggered) {
                    startDragTriggered = true;
                    //set spawn point parent to parent, and set it as last sibling
                    spawnPoint.SetParent(SlotItemDragCanvas.Singleton.transform);
                    spawnPoint.SetAsLastSibling();
                }
                spawnPoint.transform.position = eventData.position;
            }
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (topVC) {
                spawnPoint.transform.DOKill();
                dragStartPos = eventData.position;
            }
        }
        

        public void OnEndDrag(PointerEventData eventData) {
            //check if pointer is on self
            if (topVC && Vector2.Distance(eventData.position, dragStartPos) > 10) {
                this.SendCommand<SlotItemDragReleaseCommand>(
                    SlotItemDragReleaseCommand.Allocate(eventData.position, slot));
               
                spawnPoint.transform.DOMove(transform.position, 0.2f).OnComplete(() => {
                    spawnPoint.SetParent(transform);
                });
                Debug.Log("OnEndDrag");
            }
            else {
                StopDragImmediately();
            }
            startDragTriggered = false;
        }

        private void StopDragImmediately() {
            var transform1 = transform;
            spawnPoint.transform.position = transform1.position;
            spawnPoint.parent = transform1;
            startDragTriggered = false;
        }
    
        private void Clear() {
            if (topVC) {
                GameObjectPoolManager.Singleton.Recycle(topVC);
            }

            if (numberText) {
                numberText.text = "";
            }
           
            topVC = null;
            if (button) {
                button.targetGraphic = null;
            }
          
        }

        public void SetSlot(ResourceSlot slot) {
            this.slot = slot;
        }

        private void ShowItem() {
            Clear();

            IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
            int totalCount = slot.GetQuantity();
            if (topItem == null || totalCount == 0) {
                return;
            }

            string invPrefabName = topItem.InventoryVCPrefabName;
        
            SafeGameObjectPool pool =
                GameObjectPoolManager.Singleton.CreatePoolFromAB(invPrefabName, 
                    null, 5, 20, out GameObject prefab);

            topVC = pool.Allocate();
            IInventoryResourceViewController vc = topVC.GetComponent<IInventoryResourceViewController>();
            vc.InitWithID(topItem.UUID);
            
            topVC.transform.SetParent(spawnPoint);
            topVC.transform.localPosition = Vector3.zero;
            topVC.transform.localScale = Vector3.one;
            topVC.transform.SetAsFirstSibling();
            button.targetGraphic = topVC.GetComponentInChildren<Graphic>(true);
            //set left top right bottom to 10
            RectTransform rectTransform = topVC.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(10, 10);
            rectTransform.offsetMax = new Vector2(-10, -10);
        
            numberText.text = totalCount.ToString();

        }
        
        /// <summary>
        /// Only when the slot is active, the slot can show and update the item
        /// </summary>
        /// <param name="active"></param>
        public void Activate(bool active) {
            if (active) {
                ShowItem();
                slot.RegisterOnSlotUpdateCallback(OnSlotUpdate);
            }
            else {
                Clear();
                slot.UnregisterOnSlotUpdateCallback(OnSlotUpdate);
            }
        }

        private void OnSlotUpdate(string topItemID, List<string> allItems) {
            ShowItem();
        }

        private void OnDestroy() {
            slot?.UnregisterOnSlotUpdateCallback(OnSlotUpdate);
        }


        public void OnPointerEnter(PointerEventData eventData) {
            slotHoverBG.DOFade(0.5f, 0.2f);
            ResourceSlot.currentHoveredSlot = slot;
        }

        public void OnPointerExit(PointerEventData eventData) {
           slotHoverBG.DOFade(0, 0.2f);
           ResourceSlot.currentHoveredSlot = null;
        }

        private void OnDisable() {
            OnPointerExit(default);
            StopDragImmediately();
        }
    }
}
