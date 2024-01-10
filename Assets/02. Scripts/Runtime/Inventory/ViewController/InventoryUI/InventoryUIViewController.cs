using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Commands;
using Runtime.Inventory.Model;
using Runtime.UI;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Inventory.ViewController {
    public class InventoryUIViewController : AbstractPanelContainer, IController, IGameUIPanel {
        [SerializedDictionary("Title Toggle", "Panel")] [SerializeField]
        private SerializedDictionary<Toggle, RectTransform> subPanelDictionary;

        private InventorySubPanelViewController inventorySubPanelViewController;

        private RectTransform currentPanel;

        private List<Tween> tweenList = new List<Tween>();
        public override void OnInit() {
            gameObject.SetActive(true);
            inventorySubPanelViewController = GetComponentInChildren<InventorySubPanelViewController>(true);
            
            this.RegisterEvent<OnOpenSkillUpgradePanel>(OnOpenSkillUpgradePanel)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
            
            inventorySubPanelViewController.OnInit();
            
            RegisterSubPanelToggleEvents();
            gameObject.SetActive(false);
        }

        private void RegisterSubPanelToggleEvents() {
            foreach (KeyValuePair<Toggle, RectTransform> pair in subPanelDictionary) {
                pair.Value.gameObject.SetActive(true);
                pair.Key.onValueChanged.AddListener(isOn => {
                    if (isOn) {
                        SelectPanel(pair.Value);
                    }
                });
            } 
        }


        private void OnOpenSkillUpgradePanel(OnOpenSkillUpgradePanel e) {
            UIManager.Singleton.Open<SkillUpgradePanel>(this, e);
        }
        
        public override void OnOpen(UIMsg msg) {
            inventorySubPanelViewController.OnOpen(msg);
            subPanelDictionary.First().Key.SetIsOnWithoutNotify(true);
            SelectPanel(inventorySubPanelViewController.gameObject.GetComponent<RectTransform>(), true);
        }
        
        
       
        public override void OnClosed() {
            if (currentPanel != null) {
                currentPanel.GetComponent<InventorySubPanel>().OnSwitchToOtherPanel();
            }
            inventorySubPanelViewController.OnClosed();
            
            currentPanel = null;
            foreach (Tween tween in tweenList) {
                tween.Kill();
            }
            tweenList.Clear();
        }

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }


        public IPanel GetClosePanel() {
            IPanel openedChild = GetTopChild();
            if (openedChild != null) {
                return openedChild;
            }
            
            return this;
        }

        private void SelectPanel(RectTransform panel, bool moveImmediately = false) {
            foreach (Tween tween in tweenList) {
                tween.Kill();
            }

            tweenList.Clear();
            
            if (moveImmediately) {
                if (currentPanel != null) {
                    //set panel left to -1920, right to 1920
                    currentPanel.offsetMin = new Vector2(-1920, 0);
                    currentPanel.offsetMax = new Vector2(-1920, 0);
                    currentPanel.GetComponent<InventorySubPanel>().OnSwitchToOtherPanel();
                }
                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;
                panel.GetComponent<InventorySubPanel>().OnSwitchToPanel();
            }
            else {
                
                if (currentPanel != null) {
                    RectTransform targetPanel = currentPanel;
                    tweenList.Add(DOTween.To(() => targetPanel.offsetMin, x => targetPanel.offsetMin = x,
                        new Vector2(-1920, 0),
                        0.5f).SetUpdate(true));


                    tweenList.Add(DOTween.To(() => targetPanel.offsetMax, x => targetPanel.offsetMax = x,
                        new Vector2(-1920, 0),
                        0.5f).SetUpdate(true).OnKill(() => {
                        targetPanel.GetComponent<InventorySubPanel>().OnSwitchToOtherPanel();
                    }));
                }

                panel.offsetMin = new Vector2(1920, 0);
                panel.offsetMax = new Vector2(1920, 0);
               
                tweenList.Add(DOTween.To(() => panel.offsetMin, x => panel.offsetMin = x, Vector2.zero, 0.5f).SetUpdate(true));
                tweenList.Add(DOTween.To(() => panel.offsetMax, x => panel.offsetMax = x, Vector2.zero, 0.5f)
                    .SetUpdate(true));
                panel.GetComponent<InventorySubPanel>().OnSwitchToPanel();
                
                tweenList[0].OnComplete(() => {
                    tweenList.Clear();
                });
            }
            
            //move all other panels (except panel and currentPanel) to the right
            foreach (KeyValuePair<Toggle, RectTransform> pair in subPanelDictionary) {
                if (pair.Value == panel || pair.Value == currentPanel) {
                    continue;
                }

                pair.Value.offsetMin = new Vector2(1920, 0);
                pair.Value.offsetMax = new Vector2(1920, 0);
            }
            
            currentPanel = panel;
        }
    }
}
