using System.Collections;
using System.Collections.Generic;
using MikroFramework.Architecture;
using Runtime.Inventory.Model;
using UnityEngine;
using Runtime.Player;
using Runtime.Utilities.AnimationEvents;
using Runtime.Utilities.AnimatorSystem;
using MikroFramework.AudioKit;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Utilities;

namespace Runtime.Player.ViewControllers
{
    public struct OnPlayerAnimationEvent {
        public string AnimationName;
    }

    public class PlayerAnimationManager : EntityAttachedViewController<PlayerEntity>, ICanSendEvent
    {
        [SerializeField] private Animator playerAnim;

        private AnimationSMBManager animationSMBManager;
        private Dictionary<int, float> layerTargetWeight = new Dictionary<int, float>();

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            int layerCount = playerAnim.layerCount;
            for (int i = 0; i < layerCount; i++) {
                layerTargetWeight.Add(i, 0);
            }
            
            animationSMBManager = GetComponent<AnimationSMBManager>();
            this.RegisterEvent<PlayerAnimationEvent>(OnPlayerAnimationEvent);
            animationSMBManager.Event.AddListener(OnAnimationEvent);
            this.RegisterEvent<PlayerSwitchAnimEvent>(SwitchPlayerAnimLayer)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        }

        void Start() {
            
        }

        // Update is called once per frame
        void Update() {
            //lerp layer weight
            int layerCount = playerAnim.layerCount;
            //playerAnim.GetCurrentAnimatorStateInfo(0)
           
            for (int i = 0; i < layerCount; i++) {
                float currentWeight = playerAnim.GetLayerWeight(i);
                float targetWeight = layerTargetWeight[i];
                float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * 20);
                playerAnim.SetLayerWeight(i, newWeight);
            }

        }

        protected void OnAnimationEvent(string eventName)
        {
            this.SendEvent<OnPlayerAnimationEvent>(new OnPlayerAnimationEvent() {
                AnimationName = eventName
            });
            switch (eventName)
            {
                case "ReloadStart":
                    //AudioSystem.Singleton.Play2DSound("Pistol_Reload_Begin");
                    break;
                case "ReloadEnd":
                    //AudioSystem.Singleton.Play2DSound("Pistol_Reload_Finish");
                    break;
                default:
                    break;
            }
        }

        private void OnPlayerAnimationEvent(PlayerAnimationEvent e)
        {
            // Debug.Log(e.parameterName + e.flag);
            if (e.type == AnimationEventType.Trigger)
            {
                // Debug.Log("shootanim");
                playerAnim.SetTrigger(e.parameterName);
            }
            else if (e.type == AnimationEventType.Bool)
            {
                bool b;
                if ((int)e.flag == 1)
                    b = true;
                else
                    b = false;
                playerAnim.SetBool(e.parameterName, b);
            }
            else if (e.type == AnimationEventType.Float)
            {
                playerAnim.SetFloat(e.parameterName, e.flag);
            }else if (e.type == AnimationEventType.ResetTrigger) {
                playerAnim.ResetTrigger(e.parameterName);
            }else if (e.type == AnimationEventType.CrossFade) {
                for (int i = 0; i < playerAnim.layerCount; i++) {
                    playerAnim.CrossFade(e.parameterName, e.flag, i);
                }
                
            }
        }
        
        

        protected override void OnEntityFinishInit(PlayerEntity entity)
        {

        }

        protected void SwitchPlayerAnimLayer(PlayerSwitchAnimEvent e) {
            //playerAnim.SetLayerWeight(1, 0); // NoItem
            int layerCount = playerAnim.layerCount;
            HashSet<int> layersNotInList = new HashSet<int>();
            for (int i = 0; i < layerCount; i++) {
                layersNotInList.Add(i);
            }
            
            
            if (e.layerInfos != null) {
                foreach (AnimLayerInfo layerInfo in e.layerInfos) {
                    int target = playerAnim.GetLayerIndex(layerInfo.LayerName);
                    if (target == -1) {
                        Debug.LogError("Layer " + layerInfo.LayerName + " not found");
                        continue;
                    }

                    layerTargetWeight[target] = layerInfo.LayerWeight;
                    layersNotInList.Remove(target);
                }
            }
            
            foreach (int layer in layersNotInList) {
                layerTargetWeight[layer] = 0;
            }
        }
    }
}
