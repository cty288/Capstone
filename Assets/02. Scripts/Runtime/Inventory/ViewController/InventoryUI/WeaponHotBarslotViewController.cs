using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Runtime.Controls;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Inventory.ViewController;
using Runtime.Utilities;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHotBarslotViewController : ResourceSlotViewController {
	[SerializeField]
	private TMP_Text ammoText;
	[SerializeField]
	private TMP_Text weaponNameText;

	[SerializeField] private TMP_Text reloadHintText;
	private IWeaponEntity currentWeapon;
	[SerializeField] private float selectedMoveHeight = 35;

	private RectTransform slotBGRect;
	private List<Tween> tweenList = new List<Tween>();
	public override void Awake() {
		base.Awake();
		//ammoText = transform.Find("AmmoText")?.GetComponent<TMP_Text>();
		//weaponNameText = transform.Find("WeaponNameText")?.GetComponent<TMP_Text>();
		slotBGRect = slotBG.GetComponent<RectTransform>();
	}

	protected override RectTransform GetExpandedRect() {
		return spawnPoint;
	}

	protected override void OnShow(IResourceEntity topItem) {
		base.OnShow(topItem);
		if (currentWeapon != null && ammoText) {
			currentWeapon.CurrentAmmo.UnRegisterOnValueChanged(OnCurrentWeaponAmmoChange);
			currentWeapon.GetAmmoSize().RealValue.UnRegisterOnValueChanged(OnAmmoSizeChange);
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
			weapon.GetAmmoSize().RealValue.RegisterOnValueChanged(OnAmmoSizeChange)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
		}
		
		if(reloadHintText) {
			InputAction act = ClientInput.Singleton.FindActionInMaps("Reload");
			reloadHintText.text = ControlInfoFactory.Singleton.GetBindingKeyLocalizedName(act);
		}
	}

	public override void SetSelected(bool selected) {
		base.SetSelected(selected);
		if (slotBGRect) { 
			foreach (Tween tween in tweenList) {
				tween.Kill();
			}

			tweenList.Clear();
			
			float targetY = selected ? selectedMoveHeight : 0;

			tweenList.Add(DOTween.To(() => slotBGRect.offsetMin, x => slotBGRect.offsetMin = x, new Vector2(0, targetY),
				0.3f).SetUpdate(true));

			tweenList.Add(
				DOTween.To(() => slotBGRect.offsetMax, x => slotBGRect.offsetMax = x, new Vector2(0, targetY), 0.3f)
					.SetUpdate(true));
			
			tweenList[0].OnComplete(() => {
				tweenList.Clear();
			});
		}
	}

	private void OnAmmoSizeChange(int arg1, int ammoSize) {
		if (ammoText && currentWeapon != null) {
			ammoText.text = $"<size=120%>{currentWeapon.CurrentAmmo.Value}</size>/{ammoSize}";
		}
	}

	private void OnCurrentWeaponAmmoChange(int arg1, int ammo) {
		if (ammoText && currentWeapon != null) {
			ammoText.text = $"<size=120%>{ammo}</size>/{currentWeapon.GetAmmoSize().RealValue.Value}";
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
