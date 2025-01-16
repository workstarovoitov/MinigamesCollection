using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class UIUtils
{

    public static Tween UpdateMaterialColor(Material material, Color color, float duration = 0f, Action onComplete = null)
    {
        material.DOKill(); // Kill any existing tween on the material to avoid conflicts

        if (duration > 0)
        {
            return material.DOColor(color, duration)
                           .OnComplete(() => onComplete?.Invoke()); // Execute the onComplete action when the tween is done
        }
        else
        {
            material.SetColor("_Color", color); // Directly set the color without animation
            onComplete?.Invoke(); // Immediately invoke the onComplete action since there's no tween
            return null; // No tween to return if no animation was created
        }
    }

    public static Tween UpdateMaterialColor(Image image, Color color, float duration = 0f, Action onComplete = null)
    {
        Material material = image.material;
        material.DOKill(); // Kill any existing tween on the material to avoid conflicts

        if (duration > 0)
        {
            return material.DOColor(color, duration)
                            .OnUpdate(() =>
                            {
                                image.enabled = false;
                                image.enabled = true;
                            })
                           .OnComplete(() =>
                           {
                               image.enabled = false;
                               image.enabled = true;
                               onComplete?.Invoke();
                           }); // Execute the onComplete action when the tween is done
        }
        else
        {
            material.SetColor("_Color", color); // Directly set the color without animation
            image.enabled = false;
            image.enabled = true;
            onComplete?.Invoke(); // Immediately invoke the onComplete action since there's no tween
            return null; // No tween to return if no animation was created
        }
    }

    public static Tween BlinkMaterialColor(Material material, Color baseColor, float minStrength, float maxStrength, float duration, Ease ease = Ease.Linear, int loops = int.MaxValue, LoopType loopType = LoopType.Yoyo)
    {
        Color minColor = baseColor * minStrength;
        Color maxColor = baseColor * maxStrength;

        // Stop any ongoing animation on the material to avoid conflicts
        material.DOKill();
        material.DOColor(minColor, 0);
        // Create a blinking loop animation between minColor and maxColor
        return material.DOColor(maxColor, duration)
            .SetLoops(loops, loopType)
            .SetEase(ease);
    }

    public static Tween BlinkMaterialColor(Image image, Color baseColor, float minStrength, float maxStrength, float duration, Ease ease = Ease.Linear, int loops = int.MaxValue, LoopType loopType = LoopType.Yoyo)
    {
        Material material = image.material;

        Color minColor = baseColor * minStrength;
        Color maxColor = baseColor * maxStrength;

        // Stop any ongoing animation on the material to avoid conflicts
        material.DOKill();
        material.DOColor(minColor, 0);
        // Create a blinking loop animation between minColor and maxColor
        return material.DOColor(maxColor, duration)
            .SetLoops(loops, loopType)
            .OnUpdate(() =>
            {
                image.enabled = false;
                image.enabled = true;
            })
            .SetEase(ease);
    }
   
    public static bool IsPointerOverInteractibleUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        if (results.Count > 0 && IsInteractable(results[0].gameObject)) return true;
        return false;
    }

    public static bool IsPointerOverUI()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult result in results)
        {
            if (IsRaycastTarget(result.gameObject)) return true;
        }

        return false;
    }

    private static bool IsInteractable(GameObject gameObject)
    {
        // Check if the GameObject or any of its parents have an interactable UI component
        return gameObject.GetComponent<Selectable>()?.interactable ?? false;
    }
    
    public static bool IsRaycastTarget(GameObject gameObject)
    {
        Graphic graphic = gameObject.GetComponent<Graphic>();
        if (graphic != null && graphic.raycastTarget)
        {
            return true;
        }
        return false;
    }
}
