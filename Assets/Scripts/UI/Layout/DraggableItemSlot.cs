using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DraggableItemSlot : MonoBehaviour, IDropHandler
{
    public GameObject acceptedItem;
    public GameObject lastDroppedItem;  // Store reference to the last dropped item
    public UnityEvent wrongItemDropped;
    public UnityEvent correctItemDropped;

    public void OnDrop(PointerEventData eventData)
    {
        var droppedItem = eventData.pointerDrag;
        lastDroppedItem = droppedItem;  // Update lastDroppedItem on each drop
        if (droppedItem == acceptedItem)
        {
            var draggableItem = droppedItem.GetComponent<DraggableItem>();
            draggableItem.parentAfterDrag = transform;
            correctItemDropped.Invoke();
        }
        else
        {
            wrongItemDropped.Invoke();
        }
    }
}
