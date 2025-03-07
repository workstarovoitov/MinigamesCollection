using UnityEngine;

public enum CardSuit
{
    Feather,
    Sword,
    Stranger,
    Force,
    Flock,
    Empty
}

[CreateAssetMenu(fileName = "New Card Settings", menuName = "Minigame Items/Card Settings")]
public class CardSettings : ScriptableObject
{
    [SerializeField] private string cardName;
    [SerializeField] private string description;
    [SerializeField] private Sprite portraitTexture;
    [SerializeField] private CardSuit suit;

    public string CardName { get => cardName;  set => cardName = value; }
    
    public string Description { get => description; }

    public Sprite PortraitTexture { get => portraitTexture; set => portraitTexture = value; }

    public CardSuit Suit { get => suit; }
}
