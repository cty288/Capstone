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
using Runtime.Weapons.ViewControllers.CrossHairs;
using UnityEngine;
using UnityEngine.InputSystem;

public class BaseNPC :  AbstractMikroController<MainGame>, ICrossHairDetectable {
	[field: SerializeField] private Transform nameTagSpawnPoint;
	[field: SerializeField] private string localizedNameKey;
	[field: SerializeField] private Transform interactHintSpawnPoint;
	
	private InteractiveHint currentInteractiveHint;
	private bool inZone = false;

	private void Awake() {
		if (nameTagSpawnPoint == null) {
			nameTagSpawnPoint = transform;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.CompareTag("Player")) {
			inZone = true;
			GameObject hud = HUDManager.Singleton.SpawnHUDElement(interactHintSpawnPoint, "InteractHint_General",
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
		if (inZone && ClientInput.Singleton.GetSharedActions().Interact.WasPressedThisFrame()) {
			OnInteract();
		}
	}
	
	protected virtual void OnInteract() {
		
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
			HUDManager.Singleton.DespawnHUDElement(interactHintSpawnPoint, HUDCategory.InteractiveTag);
			currentInteractiveHint = null;
		}
	}

	public void OnUnPointByCrosshair() {
		HUDManager.Singleton.DespawnHUDElement(nameTagSpawnPoint, HUDCategory.NameTag);
	}

	public void OnPointByCrosshair() {
		GameObject hud = HUDManager.Singleton.SpawnHUDElement(nameTagSpawnPoint, "NameTag_General",
			HUDCategory.NameTag, true);
		INameTag nameTag = hud.GetComponent<INameTag>();
		nameTag.SetName(Localization.Get(localizedNameKey));
	}
}
