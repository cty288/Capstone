using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using Framework;
using MikroFramework.Architecture;
using Polyglot;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuildInfoPanel : AbstractMikroController<MainGame>, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private TMP_Text title;
    [SerializeField] private List<BuildPanelLevelInfo> levelInfos;
    public bool isMouseOver = false;


    private IBuffSystem buffSystem;

    private void Awake() {
        buffSystem = this.GetSystem<IBuffSystem>();
    }

    public void ShowWeapon(IWeaponEntity weapon) {
        CurrencyType mainBuildType = weapon.GetMainBuildType();
        title.text = Localization.GetFormat("BUILD_PANEL_TITLE",
            Localization.GetFormat($"BUILD_PANEL_TITLE_{mainBuildType.ToString()}"));
        Type weaponBuildType = weapon.CurrentBuildBuffType;
        
        if (weaponBuildType == null) {
            return;
        }

        if (!buffSystem.ContainsBuff(weapon, weaponBuildType, out IBuff buff)) {
            return;
        }
        
        IWeaponBuildBuff weaponBuildBuff = buff as IWeaponBuildBuff;
        if (weaponBuildBuff == null) {
            return;
        }

        string[] descriptions = weaponBuildBuff.GetAllLevelDescriptions();
        if (descriptions.Length != weaponBuildBuff.MaxLevel) {
            Debug.LogError("Descriptions length is not equal to max level!");
            return;
        }
        
        if(descriptions.Length > levelInfos.Count) {
            Debug.LogError("Descriptions length is greater than level infos count!");
            return;
        }
        
        int buildRarity = weapon.GetBuildBuffRarityFromBuildTotalRarity(weapon.GetTotalBuildRarity(mainBuildType));
        foreach (var levelInfo in levelInfos) {
            levelInfo.gameObject.SetActive(false);
        }
        
        for (int i = 0; i < descriptions.Length; i++) {
            levelInfos[i].gameObject.SetActive(true);
            
            int descriptionLevel = i + 1;
            bool disable = descriptionLevel > buildRarity;
            levelInfos[i].SetContent(descriptions[i], disable, mainBuildType);
        }
        
        StartCoroutine(RebuildLayout());
    }
    
    private IEnumerator RebuildLayout() {
        RectTransform transform = (RectTransform) this.transform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isMouseOver = false;
    }

    private void OnDisable() {
        isMouseOver = false;
    }
}
