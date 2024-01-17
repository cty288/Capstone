using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.Inventory.Commands;
using Runtime.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI {
	public abstract class SwitchableRootPanel : AbstractPanelContainer, IController, IGameUIPanel {
		 [SerializedDictionary("Title Toggle", "Panel")] [SerializeField]
        private SerializedDictionary<Toggle, RectTransform> subPanelDictionary;

     

        private RectTransform currentPanel;

        private List<Tween> tweenList = new List<Tween>();
        public override void OnInit() {
            gameObject.SetActive(true);

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



        
        public override void OnOpen(UIMsg msg) {
            SwitchableSubPanel firstSubPanel =
                subPanelDictionary.First().Value.GetComponent<SwitchableSubPanel>();
            subPanelDictionary.First().Key.SetIsOnWithoutNotify(true);
            SelectPanel(firstSubPanel.gameObject.GetComponent<RectTransform>(), true);
        }
        
        
       
        public override void OnClosed() {
            if (currentPanel != null) {
                currentPanel.GetComponent<SwitchableSubPanel>().OnSwitchToOtherPanel();
            }
           
            
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
            if(panel == currentPanel)
                return;
            foreach (Tween tween in tweenList) {
                tween.Kill();
            }

            tweenList.Clear();
            
            if (moveImmediately) {
                if (currentPanel != null) {
                    //set panel left to -1920, right to 1920
                    currentPanel.offsetMin = new Vector2(-1920, 0);
                    currentPanel.offsetMax = new Vector2(-1920, 0);
                    currentPanel.GetComponent<SwitchableSubPanel>().OnSwitchToOtherPanel();
                }
                panel.offsetMin = Vector2.zero;
                panel.offsetMax = Vector2.zero;
                OnSubpanelSelected(panel.GetComponent<SwitchableSubPanel>());
                panel.GetComponent<SwitchableSubPanel>().OnSwitchToPanel();
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
                        targetPanel.GetComponent<SwitchableSubPanel>().OnSwitchToOtherPanel();
                    }));
                }

                panel.offsetMin = new Vector2(1920, 0);
                panel.offsetMax = new Vector2(1920, 0);
               
                tweenList.Add(DOTween.To(() => panel.offsetMin, x => panel.offsetMin = x, Vector2.zero, 0.5f).SetUpdate(true));
                tweenList.Add(DOTween.To(() => panel.offsetMax, x => panel.offsetMax = x, Vector2.zero, 0.5f)
                    .SetUpdate(true));
                OnSubpanelSelected(panel.GetComponent<SwitchableSubPanel>());
                panel.GetComponent<SwitchableSubPanel>()?.OnSwitchToPanel();
                
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

        protected virtual void OnSubpanelSelected(SwitchableSubPanel panel) {
            
        }
    }
}