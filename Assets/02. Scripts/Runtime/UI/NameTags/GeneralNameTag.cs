using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public interface INameTag{
    public void SetName(string name);
}
public class GeneralNameTag : MonoBehaviour, INameTag {
    protected TMP_Text nameText;
    protected RectTransform rectTr;

    private void Awake() {
        nameText = transform.Find("NameText").GetComponent<TMP_Text>();
        rectTr = GetComponent<RectTransform>();
    }

    public void SetName(string name) {
        nameText.text = name;
        StartCoroutine(RebuildLayout());
    }
    
    private IEnumerator RebuildLayout() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTr);
    }
}
