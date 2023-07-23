using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.Event;
using MikroFramework.Managers;
using MikroFramework.Pool;
using UnityEngine;

namespace MikroFramework.BindableProperty
{
    public interface IBindableProperty {
        public object ObjectValue { get; set; }

        public IUnRegister RegisterOnObjectValueChaned(Action<object> onValueChanged);

        IUnRegister RegisterOnObjectValueChaned(Action<object, object> onValueChanged);
        
        IUnRegister RegisterWithInitObject(Action<object> onValueChanged);

        IUnRegister RegisterWithInitObject(Action<object, object> onValueChanged);

        void UnRegisterOnObjectValueChanged(Action<object> onValueChanged);
        
        void UnRegisterOnObjectValueChanged(Action<object, object> onValueChanged);
        public void UnRegisterAll();
    }
    [Serializable]
    public class BindableProperty<T> : IBindableProperty
    {
        object IBindableProperty.ObjectValue
        {
            get => Value;
            set => Value = (T) value;
        }
        

        
        public BindableProperty(T defaultValue = default) {
            this.value = defaultValue;
            actionDict = new Dictionary<Action<object>, Action<T>>();
            actionDict2 = new Dictionary<Action<object, object>, Action<T, T>>();
        }
        
        public BindableProperty() {
            actionDict = new Dictionary<Action<object>, Action<T>>();
            actionDict2 = new Dictionary<Action<object, object>, Action<T, T>>();
        }

        [ES3Serializable]
        [SerializeField]
        private T value = default(T);

        public T Value
        {
            get => value;
            set
            {
                if (value == null && this.value == null) {
                    return;
                }

                if (value != null && value.Equals(this.value)) {
                    return;
                }

                T oldValue = this.value;
                this.value = value;
                this.onValueChanged2?.Invoke(oldValue, value);
                this.onValueChanged?.Invoke(value);
            }
        }
        

        private Action<T> onValueChanged = (v) => { };
        private Action<T, T> onValueChanged2 = (v, w) => { };

        /// <summary>
        /// RegisterInstance listeners to the event that triggered when the value of the property changes.
        /// </summary>
        /// <param name="onValueChanged"></param>
        /// <returns>The returned IUnRegister allows you to call its UnRegisterWhenGameObjectDestroyed()
        /// function to unregister the event more convenient instead of calling UnRegisterOnValueChanged function</returns>
        [Obsolete("Use the one with two values (new and old) instead")]
        public IUnRegister RegisterOnValueChaned(Action<T> onValueChanged)
        {
            this.onValueChanged += onValueChanged;

            return new BindablePropertyUnRegister<T>(this, onValueChanged);

        }
        [Obsolete("Use the one with two values (new and old) instead")]
        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged) {
            onValueChanged?.Invoke(value);
            return RegisterOnValueChaned(onValueChanged);
        }


        public IUnRegister RegisterWithInitValue(Action<T,T> onValueChanged)
        {
            onValueChanged?.Invoke(default, value);
            return RegisterOnValueChaned(onValueChanged);
        }


        //a == b
        public static implicit operator T(BindableProperty<T> property) {
            return property.value;
        }

        public override string ToString() {
            return value.ToString();
        }

        /// <summary>
        /// RegisterInstance listeners to the event that triggered when the value of the property changes.
        /// </summary>
        /// <param name="onValueChanged">old and new values</param>
        /// <returns>The returned IUnRegister allows you to call its UnRegisterWhenGameObjectDestroyed()
        /// function to unregister the event more convenient instead of calling UnRegisterOnValueChanged function</returns>
        public IUnRegister RegisterOnValueChaned(Action<T, T> onValueChanged)
        {
            this.onValueChanged2 += onValueChanged;

            return new BindablePropertyUnRegister2<T>(this, onValueChanged2);

        }

        /// <summary>
        /// Unregister listeners to the event that triggered when the value of the property changes
        /// </summary>
        /// <param name="onValueChanged"></param>
        [Obsolete("Use the one with two parameters (new and old) instead")]
        public void UnRegisterOnValueChanged(Action<T> onValueChanged)
        {
            this.onValueChanged -= onValueChanged;
        }

        public void UnRegisterOnValueChanged(Action<T, T> onValueChanged)
        {
            this.onValueChanged2 -= onValueChanged;
        }



        public void UnRegisterAll() {
            this.onValueChanged = obj => { };
            this.onValueChanged2 = (obj, obj2) => { };
        }
        
        public object ObjectValue {
            get => value;
            set => Value = (T) value;
        }
        
        public IUnRegister RegisterOnObjectValueChaned(Action<object> onValueChanged) {
            return RegisterOnValueChaned((v) => { onValueChanged(v); });
        }
        
        public IUnRegister RegisterOnObjectValueChaned(Action<object, object> onValueChanged) {
            return RegisterOnValueChaned((v, w) => { onValueChanged(v, w); });
        }

        [NonSerialized]
        [ES3NonSerializable]
        private Dictionary<Action<object>, Action<T>> actionDict = new Dictionary<Action<object>, Action<T>>();
        [NonSerialized]
        [ES3NonSerializable]
        private Dictionary<Action<object, object>, Action<T, T>> actionDict2 = new Dictionary<Action<object, object>, Action<T, T>>();

        public IUnRegister RegisterWithInitObject(Action<object> onValueChanged) {
            Action<T> action = (v) => { onValueChanged(v); };
            actionDict.Add(onValueChanged, action);
            return RegisterWithInitValue(action);
        }
        
        public IUnRegister RegisterWithInitObject(Action<object, object> onValueChanged) {
            Action<T, T> action = (v, w) => { onValueChanged(v, w); };
            actionDict2.Add(onValueChanged, action);
            return RegisterWithInitValue(action);
        }

        public void UnRegisterOnObjectValueChanged(Action<object> onValueChanged) {
            if (actionDict.ContainsKey(onValueChanged)) {
                Action<T> action = actionDict[onValueChanged];
                UnRegisterOnValueChanged(action);
                actionDict.Remove(onValueChanged);
                action = null;
            }
        }
        
        public void UnRegisterOnObjectValueChanged(Action<object, object> onValueChanged) {
            if (actionDict2.ContainsKey(onValueChanged)) {
                Action<T, T> action = actionDict2[onValueChanged];
                UnRegisterOnValueChanged(action);
                actionDict2.Remove(onValueChanged);
                action = null;
            }
        }
    }
}
