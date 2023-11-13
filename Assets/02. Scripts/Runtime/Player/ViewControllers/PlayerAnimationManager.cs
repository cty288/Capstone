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

    public class PlayerAnimationManager : EntityAttachedViewController<PlayerEntity>
    {
        [SerializeField] private Animator playerAnim;

        private AnimationSMBManager animationSMBManager;

        // Start is called before the first frame update
        protected override void Awake()
        {
            base.Awake();
            animationSMBManager = GetComponent<AnimationSMBManager>();
            this.RegisterEvent<PlayerAnimationEvent>(OnPlayerAnimationEvent);
            animationSMBManager.Event.AddListener(OnAnimationEvent);
            this.RegisterEvent<PlayerSwitchAnimEvent>(SwitchPlayerAnimLayer)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        }

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        protected void OnAnimationEvent(string eventName)
        {
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
            if (e.layerInfos == null) {
                return;
            }
            
            foreach (AnimLayerInfo layerInfo in e.layerInfos) {
                playerAnim.SetLayerWeight(playerAnim.GetLayerIndex("NoItem"), 1 - layerInfo.LayerWeight); // NoItem

                int target = playerAnim.GetLayerIndex(layerInfo.LayerName);
                playerAnim.SetLayerWeight(target, layerInfo.LayerWeight);
            }
           
        }
    }
}
