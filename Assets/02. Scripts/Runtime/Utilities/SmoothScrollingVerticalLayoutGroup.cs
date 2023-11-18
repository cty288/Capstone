using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmoothScrollingVerticalLayoutGroup : MonoBehaviour
{
    public float duration = 0.5f;
    private ScrollRect scrollRect;
    private bool isLerping = false;
    private float lerpStartTime;
    //private float initialPosition;
    private float targetPosY;
    private RectTransform newItemParent;
    private VerticalLayoutGroup verticalLayoutGroup;
    private float spacing;

    //[SerializeField] private GameObject testPrefab;
    private List<GameObject> items = new List<GameObject>();

    private float originalHeight;
    private float originalPosY;
    void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        newItemParent = scrollRect.content;
        verticalLayoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
        spacing = verticalLayoutGroup.spacing;
        originalHeight = newItemParent.rect.height;
        originalPosY = newItemParent.localPosition.y;
    }

    public void AddNewItem(GameObject newItem) {
        newItem.transform.SetParent(newItemParent, false);
        newItem.transform.SetAsLastSibling();
       
        StartCoroutine(UpdateLayout(newItem));
        items.Add(newItem);
    }

    public void Clear() {
        foreach (var item in items) {
            DestroyImmediate(item);
        }
        items.Clear();
        newItemParent.sizeDelta = new Vector2(newItemParent.sizeDelta.x, originalHeight);
        var localPosition = newItemParent.localPosition;
        localPosition = new Vector3(localPosition.x, originalPosY, localPosition.z);
        newItemParent.localPosition = localPosition;
        targetPosY = originalPosY;
    }
    
    private IEnumerator UpdateLayout(GameObject newItem) {
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        yield return new WaitForEndOfFrame();
        float itemHeight = newItem.GetComponent<RectTransform>().rect.height;
        newItemParent.sizeDelta += new Vector2(0, itemHeight + spacing);

        OnContentChanged(itemHeight + spacing);
    }

    void Update() {
        var localPosition = newItemParent.localPosition;
        
        localPosition = Vector3.Lerp(localPosition,
            new Vector3(localPosition.x, targetPosY, localPosition.z),
            Time.deltaTime * duration);
        newItemParent.localPosition = localPosition;

       // Debug.Log(scrollRect.verticalNormalizedPosition);

        /*if (Input.GetKeyDown(KeyCode.Space)) {
            GameObject newElement = Instantiate(testPrefab, scrollRect.content);
            string randomString = "";
            int length = Random.Range(5, 30);
            for (int i = 0; i < length; i++) {
                randomString += Random.Range(0, 10);
            }

            newElement.GetComponent<TMP_Text>().text = randomString;
            AddNewItem(newElement);
        }
        
        if(Input.GetKeyDown(KeyCode.C)) {
            Clear();
        }*/
    }

   

    public void OnContentChanged(float newItemHeight) {
        targetPosY += newItemHeight;
        
    }
    
   
}
