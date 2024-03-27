using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.ResourceCrafting.Models;
using Runtime.UI;
using UnityEngine;

public class WeaponPartsTradingNPC : BaseNPC
{
	private BoxCollider selfSizeCollider;
	
	public BoxCollider GetSelfSizeCollider() {
		if (selfSizeCollider == null) {
			selfSizeCollider = transform.Find("SelfSizeCollider").GetComponent<BoxCollider>();
		}

		return selfSizeCollider;
	}
	protected override void OnInteract() {
		base.OnInteract();
		MainUI.Singleton.OpenOrGetClose
			<WeaponPartsTradingPanel>(MainUI.Singleton, null, false);
	}
}
