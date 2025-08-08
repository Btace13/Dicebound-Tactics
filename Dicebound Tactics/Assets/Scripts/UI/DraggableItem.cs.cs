using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Image image;
    CanvasGroup group;
    public Transform parentAfterDrag;

    private void Start()
    {
        image = GetComponent<Image>();
        group = GetComponent<CanvasGroup>();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        group.alpha = .5f;
        image.raycastTarget = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag);

        group.alpha = 1f;
        image.raycastTarget = true;
    }
}
