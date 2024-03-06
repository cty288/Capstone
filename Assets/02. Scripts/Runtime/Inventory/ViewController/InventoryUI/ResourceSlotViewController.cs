using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using DG.Tweening;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.Extensions;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Commands;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;
using Runtime.UI.NameTags;
using Runtime.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace Runtime.Inventory.ViewController {
    public class ResourceSlotViewController : AbstractMikroController<MainGame>, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler,
        IEndDragHandler, IDragHandler, IBeginDragHandler {
        [SerializeField] private TMP_Text numberText;
        private GameObject topVC = null;
        [SerializeField] protected RectTransform spawnPoint;
        private Vector2 spawnPointOriginalMinOffset;
        private Vector2 spawnPointOriginalMaxOffset;

        private ResourceSlot slot;

        private bool isHUDSlot;
        public ResourceSlot Slot => slot;
        private Vector2 dragStartPos;
        private bool startDragTriggered = false;
        private Button button;
        private Image slotHoverBG;
        private SlotResourceDescriptionPanel currentDescriptionPanel;
        private Transform descriptionPanelFollowTr;
        private RectTransform tagDetailIconSpawnPoint;

        private Transform spawnPointOriginalParent;
        //private Image selectedBG;
        protected bool isMouseOver;
        private float baseWidth;
        protected RectTransform rectTransform;
        protected bool isRightSide = false;
        protected Image slotBG;
        public static GameObject pointerDownObject = null;
        private RectTransform rarityBar;
        private ResLoader resLoader;
        private IResourceEntity topItem;

        [SerializeField] private Sprite filledSlotBG;
        [SerializeField] private Sprite unfilledSlotBG;
        [SerializeField] private float hoverBGAlpha = 1f;
        [SerializeField] private bool turnUnSelectedBGOffWhenSelected = true;
        [SerializeField] private Image selectedBG;
        [SerializeField] private bool expandWithItemWidth = false;
        [SerializeField] private float expandAdditionWidth = 0f;
        [SerializeField] private GameObject rarityIndicatorPrefab;
        [SerializeField] private bool showRarityIndicator = false;
        [SerializeField] private bool showTagDetailIcon = false;
        [SerializeField] private int maxPropertyDetailIconCount = 3;
        [SerializeField] private bool allowDrag = true;
        [SerializeField] private bool spawnDescriptionPanel = true;
        
        [SerializeField]
        protected ResourceCategory[] allowedResourceCategories = new ResourceCategory[] {
            ResourceCategory.All
        };

        public ResourceCategory[] AllowedResourceCategories {
            get => allowedResourceCategories;
            set => allowedResourceCategories = value;
        }

        public bool SpawnDescription {
            get => spawnDescriptionPanel;
            set => spawnDescriptionPanel = value;
        }

        private Action<ResourceSlotViewController> onSlotClickedCallback;

        public bool AllowDrag {
            get => allowDrag;
            set => allowDrag = value;
        }
        [SerializeField] private RectTransform cantThrowErrorMessage;
        [SerializeField] protected GameObject cantDragBG;
        
        protected virtual RectTransform GetExpandedRect() {
            return rectTransform;
        }
        public virtual void Awake() {
            if (!numberText) {
                numberText = transform.Find("InventoryItemSpawnPos/NumberText")?.GetComponent<TMP_Text>();
            }
           
            if (!spawnPoint) {
                spawnPoint = transform.Find("InventoryItemSpawnPos")?.GetComponent<RectTransform>();
            }

            rarityBar = transform.Find("RarityBar")?.GetComponent<RectTransform>();
            tagDetailIconSpawnPoint = transform.Find("TagIcon")?.GetComponent<RectTransform>();
            resLoader = this.GetUtility<ResLoader>();
            if (spawnPoint) {
                spawnPointOriginalMinOffset = spawnPoint.offsetMin;
                spawnPointOriginalMaxOffset = spawnPoint.offsetMax;
                spawnPointOriginalParent = spawnPoint.parent;
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
            
            baseWidth = GetExpandedRect().sizeDelta.x;
            ResourceSlot.currentDraggingSlot.RegisterWithInitValue(OnCurrentDraggingSlotChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            //currentDescriptionPanel.Hide();
            //descriptionPanel.Awake();
            Clear();
            
        }

        protected virtual void OnCurrentDraggingSlotChanged(ResourceSlot oldSlot, ResourceSlot newSlot) {
            if (cantDragBG) {
                if (newSlot == null) {
                    cantDragBG.SetActive(false);
                }
                else {
                    IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(newSlot.GetLastItemUUID());
                    cantDragBG.SetActive(!CanPlaceItem(topItem, true));
                }
            }
        }
        
        public virtual bool CanPlaceItem(IResourceEntity item, bool isSwapping) {
            return slot.CanPlaceItem(item, isSwapping) &&
                   (allowedResourceCategories.Contains(ResourceCategory.All)
                    || allowedResourceCategories.Contains(item.GetResourceCategory()));
        }

        private void OnEnable() {
            if (slotHoverBG) {
                slotHoverBG.color = new Color(slotHoverBG.color.r, slotHoverBG.color.g, slotHoverBG.color.b, 0);
            }
          
            StopDragImmediately();
        }

        private void Start() {
            //LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
           // baseWidth = rectTransform.sizeDelta.x;
        }

        public virtual void OnPointerClick() {
           onSlotClickedCallback?.Invoke(this);
        }
        
        public void RegisterOnSlotClickedCallback(Action<ResourceSlotViewController> callback) {
            onSlotClickedCallback += callback;
        }
        
        public void UnRegisterOnSlotClickedCallback(Action<ResourceSlotViewController> callback) {
            onSlotClickedCallback -= callback;
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
            if (!allowDrag) {
                return;
            }

            if (currentDescriptionPanel) {
                DespawnDescriptionPanel();
            }
            if (topVC && Vector2.Distance(eventData.position, dragStartPos) > 10) {
                if (!startDragTriggered) {
                    startDragTriggered = true;
                    //set spawn point parent to parent, and set it as last sibling
                    spawnPoint.SetParent(SlotItemDragCanvas.Singleton.transform);
                    spawnPoint.SetAsLastSibling();
                    ResourceSlot.currentDraggingSlot.Value = this.slot;
                }
                spawnPoint.transform.position = eventData.position;
            }

            if (startDragTriggered) {
                CheckCanThrow();
            }
        }

        private void CheckCanThrow() {
            if (!ResourceSlot.currentHoveredSlot.Value) {
                ShowCantThrowMessage(false);
                return;
            }

            ResourceSlot currentHoveredSlot = ResourceSlot.currentHoveredSlot.Value.Slot;
            if (currentHoveredSlot != null && currentHoveredSlot != slot) {
                if (currentHoveredSlot is RubbishSlot) {
                    IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
                    if (!slot.GetCanThrow(topItem)) {
                        //show error message
                        ShowCantThrowMessage(true);
                        return;
                    }
                }
            }
            
            ShowCantThrowMessage(false);
        }
        
        protected void ShowCantThrowMessage(bool show) {
            if (cantThrowErrorMessage) {
                cantThrowErrorMessage.gameObject.SetActive(show);
                StartCoroutine(RebuildLayout(cantThrowErrorMessage));
            }
        }
        
        private IEnumerator RebuildLayout(RectTransform rect) {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            yield return new WaitForEndOfFrame();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }

        public void OnBeginDrag(PointerEventData eventData) {
            if (!allowDrag) {
                return;
            }
            if (topVC) {
                spawnPoint.transform.DOKill();
                dragStartPos = eventData.position;
            }
        }
        
        
        

        public void OnEndDrag(PointerEventData eventData) {
            if (!allowDrag) {
                return;
            }
            ShowCantThrowMessage(false);
            //check if pointer is on self
            if (topVC && Vector2.Distance(eventData.position, dragStartPos) > 10) {
                this.SendCommand<SlotItemDragReleaseCommand>(
                    SlotItemDragReleaseCommand.Allocate(eventData.position, slot));
                spawnPoint.transform.position = transform.position;
                spawnPoint.SetParent(spawnPointOriginalParent);
                spawnPoint.offsetMin = spawnPointOriginalMinOffset;
                spawnPoint.offsetMax = spawnPointOriginalMaxOffset;
                /*spawnPoint.transform.DOMove(transform.position, 0.2f).OnComplete(() => {
                    spawnPoint.SetParent(transform);
                    spawnPoint.offsetMin = spawnPointOriginalMinOffset;
                    spawnPoint.offsetMax = spawnPointOriginalMaxOffset;
                });*/
                Debug.Log("OnEndDrag");
            }
            else {
                StopDragImmediately();
            }
            startDragTriggered = false;
            ResourceSlot.currentDraggingSlot.Value = null;
        }

        private void StopDragImmediately() {
            var transform1 = transform;
            spawnPoint.transform.position = transform1.position;
            spawnPoint.parent = spawnPointOriginalParent;
            spawnPoint.offsetMin = spawnPointOriginalMinOffset;
            spawnPoint.offsetMax = spawnPointOriginalMaxOffset;
            startDragTriggered = false;
            ShowCantThrowMessage(false);
            ResourceSlot.currentDraggingSlot.Value = null;
        }
    
        protected virtual void Clear() {
            ShowCantThrowMessage(false);
            
            if (topVC) {
                GameObjectPoolManager.Singleton.Recycle(topVC);
            }

            if (topItem != null) {
                topItem.GetRarityProperty().RealValue.UnRegisterOnValueChanged(OnTopItemRarityChanged);
            }

            if (numberText) {
                numberText.text = "";
            }
           
            topVC = null;
            if (button) {
                button.targetGraphic = null;
            }

            if (currentDescriptionPanel) {
                currentDescriptionPanel.SetContent("", "", null, true, 0, "", null, null,
                    null);
            }

            if (showRarityIndicator) {
                ClearRarityBar();
            }
            
            if (showTagDetailIcon) {
                for (int i = 0; i < tagDetailIconSpawnPoint.childCount; i++) {
                    GameObject go = tagDetailIconSpawnPoint.GetChild(i).gameObject;
                    Destroy(go);
                }
            }


            if (spawnDescriptionPanel) {
                DespawnDescriptionPanel();  
            }
            
        }
        
        private  void ClearRarityBar() {
            for (int i = 0; i < rarityBar.childCount; i++) {
                GameObject go = rarityBar.GetChild(i).gameObject;
                Destroy(go);
            }
        }

        public void SetSlot(ResourceSlot slot, bool isHUDSlot) {
            this.slot = slot;
            this.isHUDSlot = isHUDSlot;
        }

        protected virtual void ShowItem() {
            Clear();

            
            topItem = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
            int totalCount = slot.GetQuantity();
            if (topItem == null || totalCount == 0) {
                slotBG.sprite = unfilledSlotBG;
                GetExpandedRect().sizeDelta = new Vector2(baseWidth, baseWidth);
                StartCoroutine(RebuildLayout());
                return;
            }
            

            string invPrefabName = topItem.InventoryVCPrefabName;
        
            SafeGameObjectPool pool =
                GameObjectPoolManager.Singleton.CreatePoolFromAB(invPrefabName, 
                    null, 5, 20, out GameObject prefab);

            if (showRarityIndicator) {
                topItem.GetRarityProperty().RealValue.RegisterWithInitValue(OnTopItemRarityChanged)
                    .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            }
           
            
            topVC = pool.Allocate();
            IInventoryResourceViewController vc = topVC.GetComponent<IInventoryResourceViewController>();
            vc.InitWithID(topItem.UUID);
            vc.IsHUDSlot = isHUDSlot;

            if (spawnPoint) {
                spawnPoint.offsetMin = spawnPointOriginalMinOffset;
                spawnPoint.offsetMax = spawnPointOriginalMaxOffset;
            }

            
            topVC.transform.SetParent(spawnPoint);
            topVC.transform.localPosition = Vector3.zero;
            topVC.transform.localScale = Vector3.one;
            topVC.transform.SetAsFirstSibling();
           
            button.targetGraphic = topVC.GetComponentInChildren<Graphic>(true);
           
            RectTransform rectTransform = topVC.GetComponent<RectTransform>();
            rectTransform.offsetMin = new Vector2(0, 0);
            rectTransform.offsetMax = new Vector2(0, 0);

            if (numberText && topItem.GetMaxStackProperty().RealValue > 1) {
                numberText.text = totalCount.ToString();
            }
            
            slotBG.sprite = filledSlotBG;

            if (spawnDescriptionPanel) {
                SpawnDescriptionPanel(topItem);
            }
            

            int rarityLevel = 0;
            if (showRarityIndicator) {
                rarityLevel = topItem.GetRarityProperty().RealValue.Value;
            }

            if (showTagDetailIcon) {
                List<ResourcePropertyDescription> propertyDescriptions = topItem.GetResourcePropertyDescriptions();
                int spawnedCount = 0;
                foreach (ResourcePropertyDescription propertyDescription in propertyDescriptions) {
                    if (String.IsNullOrEmpty(propertyDescription.iconName)) continue;
                    
                    GameObject iconPrefab = resLoader.LoadSync<GameObject>(propertyDescription.iconName);

                    GameObject icon = Instantiate(iconPrefab, tagDetailIconSpawnPoint);
                    //set icon's height = propertyDetailIconSpawnPoint's height
                    float height = tagDetailIconSpawnPoint.rect.height;
                    RectTransform iconRect = icon.GetComponent<RectTransform>();
                    iconRect.sizeDelta = new Vector2(height, height);
                    iconRect.anchoredPosition = Vector2.zero;
                    
                    
                    
                    spawnedCount++;
                    if (spawnedCount >= maxPropertyDetailIconCount) {
                        break;
                    }
                }
            }
            
            
          
            if (currentDescriptionPanel) {
                Dictionary<CurrencyType, int> useCost = topItem is ISkillEntity skillEntity
                    ? skillEntity.GetSkillUseCostOfCurrentLevel()
                    : null;
                
                
                IWeaponPartsEntity weaponPartsEntity = topItem as IWeaponPartsEntity;
                CurrencyType? currencyType = null;
                if (weaponPartsEntity != null) {
                    currencyType = weaponPartsEntity.GetBuildType();
                }
                
                currentDescriptionPanel.SetContent(topItem.GetDisplayName(), topItem.GetDescription(),
                    InventorySpriteFactory.Singleton.GetSprite(topItem), !isRightSide, rarityLevel,
                    ResourceVCFactory.GetLocalizedResourceCategory(topItem.GetResourceCategory()),
                topItem.GetResourcePropertyDescriptions(),useCost,currencyType);
                
                if (ResourceSlot.currentHoveredSlot.Value == this) {
                    currentDescriptionPanel.Show();
                }
            }

            if (expandWithItemWidth) {
                float width = (baseWidth) * topItem.Width + expandAdditionWidth * (topItem.Width - 1);
                GetExpandedRect().sizeDelta = new Vector2(width, baseWidth);
                StartCoroutine(RebuildLayout());
            }

            OnShow(topItem);
        }

        private void OnTopItemRarityChanged(int arg1, int rarity) {
            ClearRarityBar();
            
            
            if (showRarityIndicator) {
                for (int i = 0; i < rarity; i++) {
                    GameObject star = Instantiate(rarityIndicatorPrefab, rarityBar);

                    if (topItem is IWeaponPartsEntity weaponPart) {
                        RarityIndicator rarityIndicator = star.GetComponent<RarityIndicator>();
                        rarityIndicator.SetCurrency(weaponPart.GetBuildType());
                    }
                        
                    RectTransform starRect = star.GetComponent<RectTransform>();
                        
                    float height = rarityBar.rect.height;
                    starRect.sizeDelta = new Vector2(height, height);
                }
            }
        }

        protected virtual void OnShow(IResourceEntity topItem) {
            
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
                currentDescriptionPanel.Clear();
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
                slot.UnregisterOnSlotUpdateCallback(OnSlotUpdate);
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
            if (spawnDescriptionPanel) {
                DespawnDescriptionPanel();
            }
            
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            isMouseOver = true;
            if (slotHoverBG) {
                slotHoverBG.DOFade(hoverBGAlpha, 0.2f).SetUpdate(true);
            }
            
            ResourceSlot.currentHoveredSlot.Value = this;
            if (slot.GetQuantity() > 0 && !currentDescriptionPanel) {
                IResourceEntity topItem = GlobalGameResourceEntities.GetAnyResource(slot.GetLastItemUUID());
                if(topItem == null) {
                    return;
                } 

                if (spawnDescriptionPanel) {
                    SpawnDescriptionPanel(topItem);
                }
              
                
                if (currentDescriptionPanel) {
                    
                    int rarityLevel = 0;
                    if (topItem.TryGetProperty(new PropertyNameInfo(PropertyName.rarity), out var rarity)) {
                        if (rarity is IRarityProperty rarityProperty) {
                            rarityLevel = rarityProperty.RealValue.Value;
                        }
                    }
                    Dictionary<CurrencyType, int> useCost = topItem is ISkillEntity skillEntity
                        ? skillEntity.GetSkillUseCostOfCurrentLevel()
                        : null;
                    
                    IWeaponPartsEntity weaponPartsEntity = topItem as IWeaponPartsEntity;
                    CurrencyType? currencyType = null;
                    if (weaponPartsEntity != null) {
                        currencyType = weaponPartsEntity.GetBuildType();
                    }
                    
                    
                    currentDescriptionPanel.SetContent(topItem.GetDisplayName(), topItem.GetDescription(),
                        InventorySpriteFactory.Singleton.GetSprite(topItem), !isRightSide, rarityLevel,
                        ResourceVCFactory.GetLocalizedResourceCategory(topItem.GetResourceCategory()),
                        topItem.GetResourcePropertyDescriptions(), useCost, currencyType);
                    currentDescriptionPanel.Show();
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData) {
            isMouseOver = false;
            if (!currentDescriptionPanel) {
                PointerExit(false);
            }
            ResourceSlot.currentHoveredSlot.Value = null;
            //PointerExit(false);
        }

        protected void PointerExit(bool closeImmediately) {
            if (slotHoverBG) {
                if (!closeImmediately) {
                    slotHoverBG.DOFade(0, 0.2f).SetUpdate(true);
                }
                else {
                    slotHoverBG.color = new Color(slotHoverBG.color.r, slotHoverBG.color.g, slotHoverBG.color.b, 0);
                }
                
            }
            
            
            if (currentDescriptionPanel) {
                currentDescriptionPanel.Hide();
                if (spawnDescriptionPanel) {
                    DespawnDescriptionPanel();
                }
              
            }
            isMouseOver = false;
        }

        private void Update() {
            if (currentDescriptionPanel) {
               if(!isMouseOver && !currentDescriptionPanel.IsMouseOverThisPanel()) {
                   PointerExit(false); 
               }
            }

            if (rarityBar) {
                float height = rarityBar.rect.height;
                for (int i = 0; i < rarityBar.childCount; i++) {
                    RectTransform starRect = rarityBar.GetChild(i).GetComponent<RectTransform>();
                    starRect.sizeDelta = new Vector2(height, height);
                }
            }
            
        }


        public void OnInventoryUIClosed() {
            PointerExit(true);
            StopDragImmediately();
            
        }
       

        public virtual void SetSelected(bool selected) {
            if (!slotBG) {
                slotBG = transform.Find("SlotBG").GetComponent<Image>();
            }
            
            if (selectedBG) {
                selectedBG.gameObject.SetActive(selected);
            }

            slotBG.gameObject.SetActive(true);
            if (selected) {
                if (turnUnSelectedBGOffWhenSelected) {
                    slotBG.gameObject.SetActive(false);
                }
            }

            //isSelected = selected;
        }

    }
}
