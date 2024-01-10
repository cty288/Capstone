using System;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
//using BehaviorDesigner.Runtime.Tasks;
using MikroFramework.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Utilities {
	[Serializable]
	public class ReferenceCounter : IRefCounter {
		[field: ES3Serializable]
		[field: SerializeField]
		public BindableProperty<int> Count { get; private set; } = new BindableProperty<int>(0);

		int IRefCounter.RefCount {
			get => Count.Value;
		}
		
		protected HashSet<object> refOwners = new HashSet<object>();
		protected Action onZeroRef;

		/// <summary>
		/// Let the current RC-1. If RC=0, trigger OnZeroRef function
		/// </summary>
		/// <param name="refOwner"></param>
		public void Release(object refOwner = null)
		{
			if (refOwner != null && !refOwners.Contains(refOwner)) {
				return;
			}

			if (refOwner != null) {
				refOwners.Remove(refOwner);
			}
			
			
			Count.Value = Mathf.Max(0, Count.Value - 1);
			if (Count.Value == 0) {
				OnZeroRef();
			}

		}


		public virtual void Retain(object refOwner = null) {
			if(refOwners.Contains(refOwner) && refOwner!=null) return;
			if (refOwner != null) {
				refOwners.Add(refOwner);
			}
			Count.Value++;
		}
		
		
		public void RegisterOnZeroRef(Action onZeroRef) {
			this.onZeroRef += onZeroRef;
		}
		
		public void UnregisterOnZeroRef(Action onZeroRef) {
			this.onZeroRef -= onZeroRef;
		}
		
		/// <summary>
		/// Triggered when the reference counter reaches 0
		/// </summary>
		protected virtual void OnZeroRef() {
			onZeroRef?.Invoke();
		}
	}
}