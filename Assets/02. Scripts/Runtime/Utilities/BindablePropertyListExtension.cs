using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using MikroFramework.Event;

namespace Runtime.Utilities {
	public class BindablePropertyExtensionUnregisterOnAdd<T> : IUnRegister
	{
		public BindableList<T> Bindable { get; set; }

		public Action<T> OnValueChanged { get; set; }

		public BindablePropertyExtensionUnregisterOnAdd(BindableList<T> bindable, Action<T> onValueChanged) {
			this.Bindable = bindable;
			this.OnValueChanged = onValueChanged;
		}

		public void UnRegister()
		{
			Bindable.UnRegisterOnAdd(OnValueChanged);
			Bindable = null;
		}
	}
	
	public class BindablePropertyExtensionUnregisterOnRemove<T> : IUnRegister
	{
		public BindableList<T> Bindable { get; set; }

		public Action<T> OnValueChanged { get; set; }

		public BindablePropertyExtensionUnregisterOnRemove(BindableList<T> bindable, Action<T> onValueChanged) {
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
	public class BindableList<T> : BindableProperty<List<T>>, IEnumerable<T> {
		public BindableList(List<T> defaultValue = default) : base(defaultValue) {
		}
		
		private Action<T> onAdd = (v) => { };
		private Action<T> onRemove = (v) => { };
		
		//allow []
		public T this[int index] {
			get => Value[index];
			set => Value[index] = value;
		}

		public void AddAndInvoke(T item) {
			Value.Add(item);
			onAdd?.Invoke(item);
		}
		
		public IUnRegister RegisterOnAdd(Action<T> onAdd) {
			this.onAdd += onAdd;
			return new BindablePropertyExtensionUnregisterOnAdd<T>(this, onAdd);
		}
		
		public void InsertAndInvoke(int index, T item) {
			Value.Insert(index, item);
			onAdd?.Invoke(item);
		}
		
		public void AddRangeAndInvoke(IEnumerable<T> collection) {
			Value.AddRange(collection);
			foreach (var item in collection) {
				onAdd?.Invoke(item);
			}
		}
		
		public void InsertRangeAndInvoke(int index, IEnumerable<T> collection) {
			Value.InsertRange(index, collection);
			foreach (var item in collection) {
				onAdd?.Invoke(item);
			}
		}
		
		public void UnRegisterOnAdd(Action<T> onAdd) {
			this.onAdd -= onAdd;
		}
		
		public bool RemoveAndInvoke(T item) {
			bool result = Value.Remove(item);
			if (result) {
				onRemove?.Invoke(item);
			}

			return result;
		}

		public void RemoveAtAndInvoke(int index) {
			T item = Value[index];
			Value.RemoveAt(index);
			onRemove?.Invoke(item);
		}
		
		public void ClearAndInvoke() {
			T[] items = Value.ToArray();
			Value.Clear();
			foreach (var item in items) {
				onRemove?.Invoke(item);
			}
		}
		
		public IUnRegister RegisterOnRemove(Action<T> onRemove) {
			this.onRemove += onRemove;
			return new BindablePropertyExtensionUnregisterOnRemove<T>(this, onRemove);
		}
		
		public void UnRegisterOnRemove(Action<T> onRemove) {
			this.onRemove -= onRemove;
		}

		public IEnumerator<T> GetEnumerator() {
			return Value.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
	}

}