using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Polyglot;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPanelPropertyDescriptionItem : AbstractMikroController<MainGame>
{
    private TMP_Text descriptionText;
    private TMP_Text titleText;
  
    private void Awake() {
        descriptionText = transform.Find("Details").GetComponent<TMP_Text>();
        titleText = transform.Find("Title").GetComponent<TMP_Text>();
    }


    public virtual void SetContent(string iconNameText, string descriptionText) {
        string colon = Localization.Get("COLON");
        if (string.IsNullOrEmpty(iconNameText)) {
            titleText.gameObject.SetActive(false);
            this.descriptionText.horizontalAlignment = HorizontalAlignmentOptions.Left;
        }
        else {
            titleText.gameObject.SetActive(true);
            titleText.text = iconNameText;
            this.descriptionText.horizontalAlignment = HorizontalAlignmentOptions.Right;
        }
        
        this.descriptionText.text = descriptionText;
        StartCoroutine(RebuildLayout());
    }
    
    private IEnumerator RebuildLayout() {
        RectTransform rt = transform as RectTransform;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }
}
