using UnityEngine;
public enum GearType { Module08, Module04 };

[CreateAssetMenu(fileName = "Gear Settings", menuName = "Minigame Items/Gears Settings")]
public class GearSettings : ScriptableObject
{
    public GearType type = GearType.Module08;
    public int teeth = 12;
    public float gearRadius = 1.525f;
    public float tipRadius = 2.275f;
    public float pinRadius = 1.25f;
    
    public Sprite baseSprite;
    public float outlineThickness = 1f;
    public float outlineInnerRadius = 0.5f;
}
