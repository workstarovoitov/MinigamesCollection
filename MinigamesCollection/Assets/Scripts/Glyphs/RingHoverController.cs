using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RingHoverController : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInParent<RingController>().HoverRing();
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
