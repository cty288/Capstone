using System.Collections;
using System.Collections.Generic;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.ViewController;
using Runtime.Utilities;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;

public class WeaponHotBarslotViewController : ResourceSlotViewController {
	private TMP_Text ammoText;
	private TMP_Text weaponNameText;
	private IWeaponEntity currentWeapon;

	protected override void Awake() {
		base.Awake();
		ammoText = transform.Find("AmmoText")?.GetComponent<TMP_Text>();
		weaponNameText = transform.Find("WeaponNameText")?.GetComponent<TMP_Text>();
	}

	protected override RectTransform GetExpandedRect() {
		return spawnPoint;
	}

	protected override void OnShow(IResourceEntity topItem) {
		base.OnShow(topItem);
		if (currentWeapon != null && ammoText) {
			currentWeapon.CurrentAmmo.UnRegisterOnValueChanged(OnCurrentWeaponAmmoChange);
			currentWeapon = null;
		}
		if (weaponNameText) {
			weaponNameText.text = topItem.GetDisplayName();
		}

		if (ammoText && topItem is IWeaponEntity weapon) {
			//ammoText.text = $"{weapon.CurrentAmmo.Value}/{weapon.GetAmmoSize().RealValue.Value}";
			currentWeapon = weapon;
			weapon.CurrentAmmo.RegisterWithInitValue(OnCurrentWeaponAmmoChange)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
		}
	}

	private void OnCurrentWeaponAmmoChange(int arg1, int ammo) {
		if (ammoText && currentWeapon != null) {
			ammoText.text = $"{ammo}/{currentWeapon.GetAmmoSize().RealValue.Value}";
		}
	}
	

	protected override void Clear() {
		base.Clear();
		if (ammoText) {
			ammoText.text = "";
		}

		if (weaponNameText) {
			weaponNameText.text = "";
		}
		
	}
}
