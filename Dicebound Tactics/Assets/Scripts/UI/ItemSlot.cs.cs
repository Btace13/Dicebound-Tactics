using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if(transform.childCount == 0)
        {
            eventData.pointerDrag.GetComponent<DraggableItem>().parentAfterDrag = transform;
        }
    }
}
