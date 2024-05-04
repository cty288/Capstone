using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Mikrocosmos
{
    public abstract class SettingsElement : MonoBehaviour {
        public abstract void OnInit();
        public abstract void OnLoad();
        public abstract void OnSave();
        public abstract void OnReset();

        public abstract void OnLateLoad();
        private void OnApplicationQuit() {
            OnSave();
        }
    }
}
