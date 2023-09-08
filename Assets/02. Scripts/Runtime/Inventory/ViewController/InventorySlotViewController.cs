using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotViewController : MonoBehaviour, IPointerClickHandler, IEndDragHandler, IDragHandler, IBeginDragHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("OnPointerClick");
    }

    public void OnEndDrag(PointerEventData eventData) {
        //check if pointer is on self
        if (eventData.pointerCurrentRaycast.gameObject != gameObject) {
            Debug.Log("OnEndDrag");
        }
    }

    public void OnDrag(PointerEventData eventData) {
        
    }

    public void OnBeginDrag(PointerEventData eventData) {
        
    }
}
