using UnityEngine;

public enum GlyphsDifficultyLevel { Fixed, Easy, Normal, Hard };

[CreateAssetMenu(fileName = "ScenarioEntityGlyphs", menuName = "Scriptable Objects/ScenarioEntityGlyphs")]
public class ScenarioEntityGlyphs : ScenarioEntity
{
    [SerializeField] private Sprite rewardBG;
    public Sprite RewardBG { get => rewardBG; }

    
    [SerializeField] private bool firstHintRandomPlaced;
    public bool FirstHintRandomPlaced { get => firstHintRandomPlaced; }
    [SerializeField] private RingsPlacementPattern hintPlacement;
    public RingsPlacementPattern HintPlacement { get => hintPlacement; }
    
    
    [SerializeField] private GlyphsDifficultyLevel settings;
    public GlyphsDifficultyLevel Settings { get => settings; }

    [SerializeField] private bool twoRingSetup;
    public bool TwoRingSetup { get => twoRingSetup; }

    [Range(2, 8)]
    [SerializeField] private int smallRingsAmount;
    public int SmallRingsAmount
    {
        get => smallRingsAmount;
        set
        {
            smallRingsAmount = Mathf.Clamp(value, 2, 8);
        }
    }
}
