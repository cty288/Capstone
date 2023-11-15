using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Controls;
using Runtime.GameResources.ViewControllers;
using Runtime.UI;
using Runtime.UI.NameTags;
using UnityEngine;
using UnityEngine.InputSystem;

public class SkillPurchaseNPC : AbstractMikroController<MainGame> {
	private InteractiveHint currentInteractiveHint;
	private bool inZone = false;
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			inZone = true;
			GameObject hud = HUDManager.Singleton.SpawnHUDElement(transform, "InteractHint_General",
				HUDCategory.InteractiveTag, true);
					
					
			if (hud) {
				currentInteractiveHint = hud.GetComponent<InteractiveHint>();
				if (currentInteractiveHint != null) {
					(InputAction action, string text, string control) = GetInteractHintInfo();
					currentInteractiveHint.SetHint(action, text, control);
				}
			}
		}
	}

	private void Update() {
		if (ClientInput.Singleton.GetSharedActions().Interact.WasPressedThisFrame()) {
			MainUI.Singleton.OpenOrGetClose<SkillPurchaseUI>(MainUI.Singleton, null);
		}
	}

	protected virtual (InputAction, string, string) GetInteractHintInfo() {
		string hint =
			Localization.Get("HINT_PRESS");

		return (ClientInput.Singleton.FindActionInSharedActionMap("Interact"),
			Localization.Get("interact"), hint);
	}
	private void OnTriggerExit(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			inZone = false;
			HUDManager.Singleton.DespawnHUDElement(transform, HUDCategory.InteractiveTag);
			currentInteractiveHint = null;
		}
	}
}
