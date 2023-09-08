using System;
using System.Collections;
using System.Collections.Generic;
using Mikrocosmos.Controls;
using MikroFramework.UIKit;
using UnityEngine;

public class MainUI : UIRoot {
	DPunkInputs.SharedActions controlActions;
	protected override void Awake() {
		base.Awake();
		controlActions = ClientInput.Singleton.GetSharedActions();
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update() {
		if (controlActions.Inventory.WasPressedThisFrame()) {
			OpenOrClose<InventoryUIViewController>(this, null);
		}

		if (controlActions.Close.WasPressedThisFrame()) {
			if (currentMainPanel != null) {
				ClosePanel(currentMainPanel);
			}
		}
	}

	public override T Open<T>(IPanelContainer parent, UIMsg message, bool createNewIfNotExist = true, string assetNameIfNotExist = "") {
		T panel = base.Open<T>(parent, message, createNewIfNotExist, assetNameIfNotExist);
		if (currentMainPanel != null) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		ClientInput.Singleton.EnableUIMaps();
		return panel;
	}
	
	public T OpenOrClose<T>(IPanelContainer parent, UIMsg message, bool createNewIfNotExist = true, string assetNameIfNotExist = "")  where T : class, IPanel {
		if (currentMainPanel!=null && currentMainPanel.GetType() == typeof(T)) {
			ClosePanel(currentMainPanel);
			return null;
		}
		
		return Open<T>(parent, message, createNewIfNotExist, assetNameIfNotExist);
	}

	public override void ClosePanel(IPanel panel, bool alsoCloseChild = true) {
		base.ClosePanel(panel, alsoCloseChild);
		if (currentMainPanel == null) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		ClientInput.Singleton.EnablePlayerMaps();
	}
}
