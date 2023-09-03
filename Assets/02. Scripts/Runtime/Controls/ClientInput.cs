using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Singletons;
using MikroFramework.UIKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;


namespace Mikrocosmos.Controls
{
    public struct OnControlMapSwitched {
        public string MapName;
    }
    public class ClientInput : MonoMikroSingleton<ClientInput>, ICanSendEvent
    {
        private DPunkInputs Inputs;
        private ClientInput(){}
        
        private PlayerInput playerInput;

        public PlayerInput PlayerInput => playerInput;

        private InputActionMap playerMap;



        //[SerializeField] private InputSystemUIInputModule gamepadInputModule;

       // public InputSystemUIInputModule GamepadInputModule => gamepadInputModule;
       // [SerializeField] private InputSystemUIInputModule keyboardInputModule;

        //public InputSystemUIInputModule KeyboardInputModule => keyboardInputModule;

        public InputAction FindActionInCurrentActionMap(string name) {
            
            InputActionMap map = PlayerInput.currentActionMap;
            InputAction action = map.FindAction(name, false);
            return action;
        }

        public InputAction FindActionInPlayerActionMap(string name) {
            if (playerMap == null) {
                playerMap = PlayerInput.actions.FindActionMap("Player");
            }
            InputAction action = playerMap.FindAction(name, false);
            return action;
        }
        public override void OnSingletonInit() {
            base.OnSingletonInit();
            Inputs = new DPunkInputs();
            playerInput = GetComponent<PlayerInput>();
            playerInput.actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString("rebinds"));
            
            var rebinds = PlayerPrefs.GetString("rebinds");
            if (!string.IsNullOrEmpty(rebinds))
                Inputs.asset.LoadBindingOverridesFromJson(rebinds);
            
            EnablePlayerMaps();
            
        }



        public DPunkInputs.UIActions GetUIActions() {
            return Inputs.UI;
        }
        public DPunkInputs.PlayerActions GetPlayerActions() {
            return  Inputs.Player;
        }

        public void EnablePlayerMaps() {
            Inputs.UI.Disable();
            Inputs.Player.Enable();
            
            if (playerInput) {
                playerInput.SwitchCurrentActionMap("Player");
                this.SendEvent<OnControlMapSwitched>(new OnControlMapSwitched()
                {
                    MapName = "Player"
                });
            }

        }

        
        public void EnableUIMaps() {
          
            Inputs.Player.Disable();
            Inputs.UI.Enable();
            if (playerInput) {
                playerInput.SwitchCurrentActionMap("UI");
                this.SendEvent<OnControlMapSwitched>(new OnControlMapSwitched()
                {
                    MapName = "UI"
                });
            }

        }
        
        /*private void Update() {
            if (playerInput && playerInput.currentControlScheme == "Gamepad") {
                keyboardInputModule.enabled = false;
                gamepadInputModule.enabled = true;

            }
            else
            {
                keyboardInputModule.enabled = true;
                gamepadInputModule.enabled = false;
            }
        }*/
        

        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }
    }

}
