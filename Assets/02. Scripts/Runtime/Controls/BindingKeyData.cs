using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Controls
{

    //action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);

    
    [Serializable]
    public class BindingInfo {
        public string ControlPath;
        public string LocalizationKey;
        public Sprite Sprite;
    }

    [Serializable]
    public class DeviceBindingData {
        public string DeviceName;
        public List<BindingInfo> bindingInfos;

        public BindingInfo GetBindingInfo(string controlPath) {
            return bindingInfos.FirstOrDefault(x => x.ControlPath == controlPath);
        }
    }
    
    [Serializable]
    public struct KeyboardMouseBindingData {
        
        public Sprite rightButton;
        public Sprite leftButton;
        public Sprite middleButton;
        public Sprite scroll;
        public Sprite up;
        public Sprite down;
        public Sprite left;
        public Sprite right;
        public Sprite space;
        public Sprite enter;
        public Sprite backSpace;
        public Sprite tab;
        public Sprite shift;

    

        [Obsolete]
        public Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            switch (controlPath)
            {
                case "leftButton": return leftButton;
                case "rightButton": return rightButton;
                case "middleButton": return middleButton;
                case "scroll/x": return scroll;
                case "scroll/y": return scroll;
                case "upArrow": return up;
                case "downArrow": return down;
                case "leftArrow": return left;
                case "rightArrow": return right;
                case "space": return space;
                case "enter": return enter;
                case "backspace": return backSpace;
                case "tab": return tab;
                case "shift": return shift;
                case "leftShift": return shift;
                case "rightShift": return shift;
                default: return null;
            }
            return null;
        }
    }

    [Serializable]
    public struct GamepadBindingData
    {
        public Sprite buttonSouth;
        public Sprite buttonNorth;
        public Sprite buttonEast;
        public Sprite buttonWest;
        public Sprite startButton;
        public Sprite selectButton;
        public Sprite leftTrigger;
        public Sprite rightTrigger;
        public Sprite leftShoulder;
        public Sprite rightShoulder;
        public Sprite dpad;
        public Sprite dpadUp;
        public Sprite dpadDown;
        public Sprite dpadLeft;
        public Sprite dpadRight;
        public Sprite leftStick;
        public Sprite rightStick;
        public Sprite leftStickPress;
        public Sprite rightStickPress;

        public Sprite GetSprite(string controlPath)
        {
            // From the input system, we get the path of the control on device. So we can just
            // map from that to the sprites we have for gamepads.
            switch (controlPath)
            {
                case "buttonSouth": return buttonSouth;
                case "buttonNorth": return buttonNorth;
                case "buttonEast": return buttonEast;
                case "buttonWest": return buttonWest;
                case "start": return startButton;
                case "select": return selectButton;
                case "leftTrigger": return leftTrigger;
                case "rightTrigger": return rightTrigger;
                case "leftShoulder": return leftShoulder;
                case "rightShoulder": return rightShoulder;
                case "dpad": return dpad;
                case "dpad/up": return dpadUp;
                case "dpad/down": return dpadDown;
                case "dpad/left": return dpadLeft;
                case "dpad/right": return dpadRight;
                case "leftStick": return leftStick;
                case "rightStick": return rightStick;
                case "leftStickPress": return leftStickPress;
                case "rightStickPress": return rightStickPress;
            }
            return null;
        }
    }

    

    [CreateAssetMenu(fileName = "BindingData")]
    public class BindingKeyData : ScriptableObject {
        public List<DeviceBindingData> BindingDatas;

        public BindingInfo GetBindingInfo(InputAction action, out string internalDisplayName, string group = null) {
            int bindingIndex = action.GetBindingIndex(group: group);

            internalDisplayName = action.GetBindingDisplayString(bindingIndex,
                            out string hookDeviceLayoutName, out string hookControlPath,
                InputBinding.DisplayStringOptions.DontIncludeInteractions);

          
            return GetBindingInfoFromDeviceNameAndControlPath(hookDeviceLayoutName, hookControlPath);
        }

        public BindingInfo GetBindingInfoFromDeviceNameAndControlPath(string deviceName, string controlPath) {
            if (InputSystem.IsFirstLayoutBasedOnSecond(deviceName, "DualShockGamepad"))
                deviceName = "DualShockGamepad";
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceName, "GamePad"))
                deviceName = "GamePad";


            var bindingData = BindingDatas.FirstOrDefault(x => x.DeviceName == deviceName);
            BindingInfo info = bindingData?.GetBindingInfo(controlPath);
            if (info == null) {
                info = new BindingInfo() {ControlPath = controlPath, LocalizationKey = "", Sprite = null};
            }

            return info;
        }
    }
}
