using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RingHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] private bool unhoverOnExit = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInParent<RingController>().HoverRing();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!unhoverOnExit) return;
        
        GetComponentInParent<RingController>().UnhoverRing();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GetComponentInParent<RingController>().ClickRingClockwise();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            GetComponentInParent<RingController>().ClickRingCounterClockwise();
        }
        //GetComponentInParent<RingController>().ClickRing();
    }
}
