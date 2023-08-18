using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Event;

namespace Runtime.Utilities {
	
	public class BindableHashsetExtensionUnregisterOnAdd<T> : IUnRegister {
		
		public BindableHashset<T> Bindable { get; set; }

		public Action<T> OnValueChanged { get; set; }

		public BindableHashsetExtensionUnregisterOnAdd(BindableHashset<T> bindable, Action<T> onValueChanged) {
			this.Bindable = bindable;
			this.OnValueChanged = onValueChanged;
		}

		public void UnRegister()
		{
			Bindable.UnRegisterOnAdd(OnValueChanged);
			Bindable = null;
		}
	}
	
	public class BindableHashsetExtensionUnregisterOnRemove<T> : IUnRegister
	{
		public BindableHashset<T> Bindable { get; set; }

		public Action<T> OnValueChanged { get; set; }

		public BindableHashsetExtensionUnregisterOnRemove(BindableHashset<T> bindable, Action<T> onValueChanged) {
			this.Bindable = bindable;
			this.OnValueChanged = onValueChanged;
		}

		public void UnRegister()
		{
			Bindable.UnRegisterOnRemove(OnValueChanged);
			Bindable = null;
		}
	}
	
	
	
	[Serializable]
	public class BindableHashset<T> : BindableProperty<HashSet<T>>, IEnumerable<T> {
		public BindableHashset(HashSet<T> defaultValue = default) : base(defaultValue) {
			
		}
		
		private Action<T> onAdd = (v) => { };
		private Action<T> onRemove = (v) => { };

		
		public void AddAndInvoke(T item) {
			Value.Add(item);
			onAdd?.Invoke(item);
		}
		
		public void RemoveAndInvoke(T item) {
			Value.Remove(item);
			onRemove?.Invoke(item);
		}
		
		public void RegisterOnAdd(Action<T> onAdd) {
			this.onAdd += onAdd;
		}
		
		public void RegisterOnRemove(Action<T> onRemove) {
			this.onRemove += onRemove;
		}
		
		public void UnRegisterOnAdd(Action<T> onAdd) {
			this.onAdd -= onAdd;
		}
		
		public void UnRegisterOnRemove(Action<T> onRemove) {
			this.onRemove -= onRemove;
		}
		
		public void ClearAndInvoke() {
			foreach (var item in Value) {
				onRemove?.Invoke(item);
			}
			Value.Clear();
		}
		
		public bool Contains(T item) {
			return Value.Contains(item);
		}
		
		public int Count => Value.Count;
		
		
		public IEnumerator<T> GetEnumerator() {
			return Value.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}
}