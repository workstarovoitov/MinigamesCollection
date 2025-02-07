using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class BackgroundHoverController : MonoBehaviour, IPointerEnterHandler
{
    public static Action bgHovered;
    public void OnPointerEnter(PointerEventData eventData)
    {
        bgHovered?.Invoke();
    }
}
