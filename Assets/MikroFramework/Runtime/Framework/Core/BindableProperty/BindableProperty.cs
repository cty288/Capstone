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

        public IUnRegister RegisterValueChaned(Action<object> onValueChanged);

        IUnRegister RegisterValueChaned(Action<object, object> onValueChanged);
        
        IUnRegister RegisterWithInit(Action<object> onValueChanged);

        IUnRegister RegisterWithInit(Action<object, object> onValueChanged);

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
        
        public IUnRegister RegisterValueChaned(Action<object> onValueChanged) {
            return RegisterOnValueChaned((v) => { onValueChanged(v); });
        }
        
        public IUnRegister RegisterValueChaned(Action<object, object> onValueChanged) {
            return RegisterOnValueChaned((v, w) => { onValueChanged(v, w); });
        }

        
        public IUnRegister RegisterWithInit(Action<object> onValueChanged) {
            return RegisterWithInitValue((v) => { onValueChanged(v); });
        }
        
        public IUnRegister RegisterWithInit(Action<object, object> onValueChanged) {
            return RegisterWithInitValue((v, w) => { onValueChanged(v, w); });
        }
        
        
    }
}
