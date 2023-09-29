using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Extensions;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Commands;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;
using Runtime.UI.NameTags;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Runtime.Inventory.ViewController {
    public class ResourceSlotViewController : AbstractMikroController<MainGame>, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
        IEndDragHandler, IDragHandler, IBeginDragHandler {
        private TMP_Text numberText;
        private GameObject topVC = null;
        private RectTransform spawnPoint;
        private Vector2 spawnPointOriginalMinOffset;
        private Vector2 spawnPointOriginalMaxOffset;
        
        private ResourceSlot slot;
        

        public ResourceSlot Slot => slot;
        private Vector2 dragStartPos;
        private bool startDragTriggered = false;
        private Button button;
        private Image slotHoverBG;
        private SlotResourceDescriptionPanel currentDescriptionPanel;
        private Transform descriptionPanelFollowTr;
        //private Image selectedBG;
        protected bool isSelected = false;
        private float baseWidth;
        protected RectTransform rectTransform;
        protected bool isRightSide = false;
        protected Image slotBG;
        public static GameObject pointerDownObject = null;

        [SerializeField] private Sprite filledSlotBG;
        [SerializeField] private Sprite unfilledSlotBG;
        [SerializeField] private float hoverBGAlpha = 1f;
        [SerializeField] private bool turnUnSelectedBGOffWhenSelected = true;
        [SerializeField] private Image selectedBG;
        [SerializeField] private bool expandWithItemWidth = false;
        [SerializeField] private float expandAdditionWidth = 0f;
        protected virtual void Awake() {
            numberText = transform.Find("InventoryItemSpawnPos/NumberText").GetComponent<TMP_Text>();
            spawnPoint = transform.Find("InventoryItemSpawnPos")?.GetComponent<RectTransform>();
            if (spawnPoint) {
                spawnPointOriginalMinOffset = spawnPoint.offsetMin;
                spawnPointOriginalMaxOffset = spawnPoint.offsetMax;
            }
            slotBG = transform.Find("SlotBG").GetComponent<Image>();
            slotHoverBG = transform.Find("SlotHoverBG")?.GetComponent<Image>();
            //descriptionPanel = transform.Find("DescriptionTag").GetComponent<SlotResourceDescriptionPanel>();
            button = GetComponent<Button>();
            descriptionPanelFollowTr = transform.Find("DescriptionFollowTr");
            //currentDescriptionPanel = HUDManagerUI.Singleton
               // .SpawnHUDElement(descriptionPanelFollowTr, "DescriptionTag", HUDCategory.SlotDescription, false)
                //.GetComponent<SlotResourceDescriptionPanel>();
            //selectedBG = transform.Find("SelectedBG")?.GetComponent<Image>();
            if (selectedBG) {
                selectedBG.gameObject.SetActive(false);
            }
            rectTransform = GetComponent<RectTransform>();
            
            baseWidth = rectTransform.sizeDelta.x;
            //currentDescriptionPanel.Hide();
            //descriptionPanel.Awake();
            Clear();
        }

        private void Start() {
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
           // baseWidth = rectTransform.sizeDelta.x;
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

            if (currentDescriptionPanel) {
                currentDescriptionPanel.SetContent("", "", null, true);
            }
            DespawnDescriptionPanel();  
            
          
        }

        public void SetSlot(ResourceSlot slot) {
            this.slot = slot;
        }

        private void ShowItem() {
            Clear();

            
            IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
            int totalCount = slot.GetQuantity();
            if (topItem == null || totalCount == 0) {
                slotBG.sprite = unfilledSlotBG;
                this.rectTransform.sizeDelta = new Vector2(baseWidth, baseWidth);
                StartCoroutine(RebuildLayout());
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
            slotBG.sprite = filledSlotBG;

            SpawnDescriptionPanel(topItem);
            if (currentDescriptionPanel) {
                currentDescriptionPanel.SetContent(topItem.GetDisplayName(), topItem.GetDescription(),
                    InventorySpriteFactory.Singleton.GetSprite(topItem.IconSpriteName), !isRightSide);
                
                if (ResourceSlot.currentHoveredSlot == this) {
                    currentDescriptionPanel.Show();
                }
            }

            if (expandWithItemWidth) {
                float width = (baseWidth) * topItem.Width + expandAdditionWidth * (topItem.Width - 1);
                this.rectTransform.sizeDelta = new Vector2(width, baseWidth);
                StartCoroutine(RebuildLayout());
            }
        }
        
        private void SpawnDescriptionPanel(IResourceEntity topItem) {
            if (currentDescriptionPanel) {
                DespawnDescriptionPanel();
            }
           
            currentDescriptionPanel = HUDManagerUI.Singleton
                .SpawnHUDElement(descriptionPanelFollowTr, topItem.Width <= 1 ? "DescriptionTag" : "DescriptionTag_Long"
                    , HUDCategory.SlotDescription, false)
                .GetComponent<SlotResourceDescriptionPanel>();
            currentDescriptionPanel.Hide();
            
        }
        
        private void DespawnDescriptionPanel() {
            if (currentDescriptionPanel) {
                currentDescriptionPanel.Hide();
                HUDManagerUI.Singleton.DespawnHUDElement(descriptionPanelFollowTr, HUDCategory.SlotDescription);
            }
            currentDescriptionPanel = null;
        }
        
        private IEnumerator RebuildLayout() {
            //set left right top bottom to 0
            spawnPoint.GetComponent<RectTransform>().offsetMin = spawnPointOriginalMinOffset;
            spawnPoint.GetComponent<RectTransform>().offsetMax = spawnPointOriginalMaxOffset;
            
            RectTransform parent = rectTransform.parent as RectTransform;
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }
        
        /// <summary>
        /// Only when the slot is active, the slot can show and update the item
        /// </summary>
        /// <param name="active"></param>
        public void Activate(bool active, bool isRightSideSlot) {
            this.isRightSide = isRightSideSlot;
            if (active) {
                ShowItem();
                slot.RegisterOnSlotUpdateCallback(OnSlotUpdate);
            }
            else {
                Clear();
                slot.UnregisterOnSlotUpdateCallback(OnSlotUpdate);
            }
        }

        private void OnSlotUpdate(ResourceSlot slot, string topItemID, List<string> allItems) {
            ShowItem();
        }

        private void OnDestroy() {
            slot?.UnregisterOnSlotUpdateCallback(OnSlotUpdate);
            DespawnDescriptionPanel();
        }


        public void OnPointerEnter(PointerEventData eventData) {
            if (slotHoverBG) {
                slotHoverBG.DOFade(hoverBGAlpha, 0.2f);
            }
            
            ResourceSlot.currentHoveredSlot = this;
            if (slot.GetQuantity() > 0) {
                IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
                if(topItem == null) {
                    return;
                }
                SpawnDescriptionPanel(topItem);
                if (currentDescriptionPanel) {
                    currentDescriptionPanel.SetContent(topItem.GetDisplayName(), topItem.GetDescription(),
                        InventorySpriteFactory.Singleton.GetSprite(topItem.IconSpriteName), !isRightSide);
                    currentDescriptionPanel.Show();
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (slotHoverBG) {
                slotHoverBG.DOFade(0, 0.2f);
            }
            
           ResourceSlot.currentHoveredSlot = null;
           if (currentDescriptionPanel) {
               currentDescriptionPanel.Hide();
               DespawnDescriptionPanel();
           }
        }

        private void Update() {
            if (ResourceSlot.currentHoveredSlot == this) {
                descriptionPanelFollowTr.position = Input.mousePosition;
            }
            
            
        }
        
        

        private void OnDisable() {
            OnPointerExit(default);
            StopDragImmediately();
        }

        public virtual void SetSelected(bool selected) {
            if (selectedBG) {
                selectedBG.gameObject.SetActive(selected);
            }

            slotBG.gameObject.SetActive(true);
            if (selected) {
                if (turnUnSelectedBGOffWhenSelected) {
                    slotBG.gameObject.SetActive(false);
                }
            }

            isSelected = selected;
        }

    }
}
