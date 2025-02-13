using UnityEngine;

public class GearLayerController : MonoBehaviour
{
    [SerializeField] private bool isGear = true;
    public bool IsGear { get => isGear; set => isGear = value; }

    [SerializeField] private SpriteRenderer srBase;
    [SerializeField] private SpriteRenderer srShadowBase;
    [SerializeField] private SpriteRenderer srDeco;
    [SerializeField] private SpriteRenderer srShadowDeco;
    
    [SerializeField] private SpriteRenderer srOutline;

    public void SetLayer(int newLayer)
    {
        if (isGear) SetGearLayer(newLayer);
        else SetPinLayer(newLayer);
    }

    private void SetGearLayer(int newLayer)
    {
        if (srOutline) srOutline.sortingOrder = newLayer * 10;
        if (srShadowBase) srShadowBase.sortingOrder = newLayer * 10 + 1;
        if (srBase) srBase.sortingOrder = newLayer  * 10 + 2;
        if (srShadowDeco) srShadowDeco.sortingOrder = newLayer * 10 + 3;
        if (srDeco) srDeco.sortingOrder = newLayer * 10 + 4;
    }

    private void SetPinLayer(int newLayer)
    {
        if (srShadowBase) srShadowBase.sortingOrder = newLayer * 10 - 5;
        if (srBase) srBase.sortingOrder = newLayer  * 10 - 4;
        if (srOutline) srOutline.sortingOrder = newLayer * 10 - 3;
        if (srShadowDeco) srShadowDeco.sortingOrder = newLayer * 10 - 2;
        if (srDeco) srDeco.sortingOrder = newLayer * 10 - 1;
    }

    public void SetBaseSprite(Sprite sprite)
    {
        if (srBase) srBase.sprite = sprite;
        if (srShadowBase) srShadowBase.sprite = sprite;
    }

    public void SetOutlineSprite(Sprite sprite)
    {
        if (srOutline) srOutline.sprite = sprite;
    }
}
