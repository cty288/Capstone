using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Event;

namespace _02._Scripts.Runtime.Utilities {
	
	public class BindableDictExtensionUnregisterOnRemove<TKey, TValue> : IUnRegister
	{
		public BindableDictionary<TKey, TValue> Bindable { get; set; }

		public Action<TKey, TValue> OnValueChanged { get; set; }

		public BindableDictExtensionUnregisterOnRemove(BindableDictionary<TKey, TValue> bindable, Action<TKey, TValue> onValueChanged) {
			this.Bindable = bindable;
			this.OnValueChanged = onValueChanged;
		}

		public void UnRegister() {
			Bindable.UnRegisterOnRemove(OnValueChanged);
			Bindable = null;
		}
	}
	
	public class BindableDictExtensionUnregisterOnAdd<TKey, TValue> : IUnRegister
	{
		public BindableDictionary<TKey, TValue> Bindable { get; set; }

		public Action<TKey, TValue> OnValueChanged { get; set; }

		public BindableDictExtensionUnregisterOnAdd(BindableDictionary<TKey, TValue> bindable, Action<TKey, TValue> onValueChanged) {
			this.Bindable = bindable;
			this.OnValueChanged = onValueChanged;
		}

		public void UnRegister() {
			Bindable.UnRegisterOnAdd(OnValueChanged);
			Bindable = null;
		}
	}
	
	
	
	public class BindableDictionary<TKey, TValue> : BindableProperty<Dictionary<TKey, TValue>>,
		IEnumerable<KeyValuePair<TKey, TValue>> {
		public BindableDictionary(Dictionary<TKey, TValue> defaultValue = default) : base(defaultValue) {
		}

		private Action<KeyValuePair<TKey, TValue>> onAdd = (v) => { };
		private Action<KeyValuePair<TKey, TValue>> onRemove = (v) => { };
		
		//allow []
		public TValue this[TKey key] {
			get => Value[key];
			set => Value[key] = value;
		}
		
		public void AddAndInvoke(KeyValuePair<TKey, TValue> item) {
			Value.Add(item.Key, item.Value);
			onAdd.Invoke(item);
		}
		
		public void AddAndInvoke(TKey key, TValue value) {
			Value.Add(key, value);
			onAdd.Invoke(new KeyValuePair<TKey, TValue>(key, value));
		}
		
		public void RemoveAndInvoke(KeyValuePair<TKey, TValue> item) {
			Value.Remove(item.Key);
			onRemove.Invoke(item);
		}
		
		public void RemoveAndInvoke(TKey key) {
			Value.Remove(key);
			onRemove.Invoke(new KeyValuePair<TKey, TValue>(key, Value[key]));
		}
		
		public BindableDictExtensionUnregisterOnAdd<TKey, TValue> RegisterOnAdd(Action<TKey, TValue> onAdd) {
			this.onAdd += (v) => onAdd.Invoke(v.Key, v.Value);
			return new BindableDictExtensionUnregisterOnAdd<TKey, TValue>(this, onAdd);
		}
		
		public BindableDictExtensionUnregisterOnRemove<TKey, TValue> RegisterOnRemove(Action<TKey, TValue> onRemove) {
			this.onRemove += (v) => onRemove.Invoke(v.Key, v.Value);
			return new BindableDictExtensionUnregisterOnRemove<TKey, TValue>(this, onRemove);
		}
		
		public void UnRegisterOnAdd(Action<TKey, TValue> onAdd) {
			this.onAdd -= (v) => onAdd.Invoke(v.Key, v.Value);
		}
		
		public void UnRegisterOnRemove(Action<TKey, TValue> onRemove) {
			this.onRemove -= (v) => onRemove.Invoke(v.Key, v.Value);
		}
		
		public void ClearAndInvoke() {
			KeyValuePair<TKey, TValue>[] items = new KeyValuePair<TKey, TValue>[Value.Count];
			int index = 0;
			foreach (TKey key in Value.Keys) {
				items[index] = new KeyValuePair<TKey, TValue>(key, Value[key]);
				index++;
			}
			Value.Clear();
			foreach (KeyValuePair<TKey, TValue> item in items) {
				onRemove.Invoke(item);
			}
		}


		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
			return Value.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}