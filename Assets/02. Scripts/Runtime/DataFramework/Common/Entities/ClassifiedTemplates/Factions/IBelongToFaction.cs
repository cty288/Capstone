using MikroFramework.BindableProperty;
using UnityEngine;

namespace Runtime.DataFramework.Entities.ClassifiedTemplates.Factions {


	
	public interface IBelongToFaction {
		/// <summary>
		/// Faction can be changed during runtime
		/// </summary>
		public BindableProperty<Faction> CurrentFaction { get; }


		/// <summary>
		/// Detect if the other entity is in the same faction
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool IsSameFaction(IBelongToFaction other) {
			if (other == null)
			{
				Debug.Log("Cannot check faction: Other is null");
			}
			return CurrentFaction.Value == other.CurrentFaction.Value;
		}
	}
}