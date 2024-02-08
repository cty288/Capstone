using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using TMPro;
using UnityEngine;

public class BuildPanelLevelInfo : MonoBehaviour {
    [SerializeField] private List<RarityIndicator> rarityIndicators;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Color disabledColor;


    public void SetContent(string description, bool disable, CurrencyType buildType) {
        this.description.text = description;
        foreach (var rarityIndicator in rarityIndicators) {
            if (disable) {
                rarityIndicator.SetColor(disabledColor);
            }else {
                rarityIndicator.SetCurrency(buildType);
            }
        }

        this.description.color = disable ? disabledColor : Color.black;
    }
}
