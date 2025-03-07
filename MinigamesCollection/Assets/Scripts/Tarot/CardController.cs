using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;
using DG.Tweening;

public class CardController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private CardSettings cardSettings;

    [Header("SFX")]
    [SerializeField] private EventReference getEventRef;
    [SerializeField] private EventReference dropEventRef;
    [SerializeField] private EventReference slideEventRef;
    [SerializeField] private EventReference swapEventRef;
    [SerializeField] private EventReference zoomInEventRef;
    [SerializeField] private EventReference zoomOutEventRef;

    [Header("Images")]
    [SerializeField] private SpriteRenderer portrait;
    [SerializeField] private SpriteRenderer background;
    [SerializeField] private SpriteRenderer glow;
    [SerializeField] private float transitionDuration;

    [Header("Sorting order layers")]
    [SerializeField] private int transitionTopSortingOrder;
    [SerializeField] private int transitionBottomSortingOrder;

    [SerializeField] private Vector3 zoomPoint = Vector3.zero;
    [SerializeField] private float zoomFocused = 3f;
    [SerializeField] private float zoomSelected = 1.25f;
    private bool zoomIsActive = false;
    public bool ZoomIsActive => zoomIsActive;
    public CardSettings CardSettings { get => cardSettings; set { cardSettings = value; UpdateImages(); } }

    private CardSlot cardSlotCurrent = null;
    private CardSlot cardLastSlot = null;

    public CardSlot CardSlotCurrent { get => cardSlotCurrent; set { cardLastSlot = cardSlotCurrent; cardSlotCurrent = value; } }
    public CardSlot CardSlotOld { get => cardLastSlot; set => cardLastSlot = value; }

    private Vector3 offset;
    private Vector3 startPosition;
    public Vector3 StartPosition { get => startPosition; set => startPosition = value; }
    private int startLayer = 0;
    private Tweener outlineTween;

    private SimpleMover simpleMover;
    private SimpleScaler simpleScaler;

    private const float CardDepthOffset = 0.5f;
    
    private Camera sceneCamera;
    public Camera SceneCamera { set => sceneCamera = value; }

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        simpleMover = GetComponent<SimpleMover>();
        simpleScaler = GetComponent<SimpleScaler>();

        if (simpleMover == null || simpleScaler == null)
        {
            Debug.LogError("Failed to initialize SimpleMover or SimpleScaler.");
        }
    }

    public void UpdateCardSide(int newLayer = 0, bool showBackground = false)
    {
        if (newLayer == 0) newLayer = startLayer;
        if (startLayer == 0) startLayer = newLayer;

        portrait.sortingOrder = newLayer * 10;
        glow.sortingOrder = newLayer * 10 - 1;
        background.sortingOrder = newLayer * 10 + 1;
        background.enabled = showBackground;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateCardSide(transitionTopSortingOrder);

        if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
        {
            ZoomCard();
            return;
        }

        HandleLeftClick(eventData);
    }

    public void ZoomCard(bool zoomIn = true)
    {
        SoundManager.Instance.Shoot(zoomIn ? zoomInEventRef : zoomOutEventRef);

        zoomIsActive = zoomIn;
        simpleMover.StopAnimation();
        simpleMover.TargetPoint = zoomIn ? zoomPoint : (cardSlotCurrent == null ? startPosition : cardSlotCurrent.transform.position);
        simpleMover.RunAnimation(() => { if (!zoomIn) UpdateCardSide(); });

        simpleScaler.StopAnimation();
        simpleScaler.TargetScale = zoomIn ? zoomFocused : simpleScaler.StartScale;
        simpleScaler.RunAnimation();
    }

    private void HandleLeftClick(PointerEventData eventData)
    {
        ShowOutline();

        simpleMover.StopAnimation();
        Vector3 pointerWorldPosition = eventData != null ? sceneCamera.ScreenToWorldPoint(eventData.position) : transform.position;
        offset = transform.position - pointerWorldPosition;
        transform.position = pointerWorldPosition + offset;

        if (cardSlotCurrent) cardSlotCurrent.Card = null;
        CardSlotCurrent = null;
        TarotSceneController.Instance.UpdateEdges();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ZoomCard(false);
            return;
        }

        HandleLeftClickRelease();
    }

    private void HandleLeftClickRelease()
    {
        ShowOutline(false);

        simpleScaler.StopAnimation();
        simpleScaler.TargetScale = simpleScaler.StartScale;
        simpleScaler.RunAnimation();

        SoundManager.Instance.Shoot(slideEventRef);
        FixCardPosition();
    }

    public void FixCardPosition()
    {
        var boxCollider = GetComponent<BoxCollider2D>();
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxCollider.size * 0.5f, transform.rotation.eulerAngles.z);

        foreach (Collider2D collider in colliders)
        {
            CardSlot newCardSlot = collider.GetComponent<CardSlot>();
            if (newCardSlot == null) continue;

            if (newCardSlot.IsFree)
            {
                PlaceCardInSlot(newCardSlot);
            }
            else
            {
                HandleOccupiedSlot(newCardSlot);
            }
            return;
        }

        ReturnCardToLastSlot();
    }

    public void HandleOccupiedSlot(CardSlot newCardSlot)
    {
        CardController occupiedCard = newCardSlot.Card;
        occupiedCard.UpdateCardSide(transitionBottomSortingOrder);
        SoundManager.Instance.Shoot(swapEventRef);

        if (cardLastSlot == null)
        {
            ReturnOccupiedCardToStartPosition(occupiedCard);
        }
        else
        {
            MoveOccupiedCardToLastSlot(occupiedCard);
        }
        PlaceCardInSlot(newCardSlot);
    }

    private void MoveOccupiedCardToLastSlot(CardController occupiedCard)
    {
        occupiedCard.CardSlotCurrent = cardLastSlot;
        occupiedCard.simpleMover.TargetPoint = cardLastSlot.transform.position + Vector3.back * CardDepthOffset;
        occupiedCard.simpleMover.RunAnimation(() =>
        {
            occupiedCard.UpdateCardSide();
            occupiedCard.CardSlotCurrent.ShowParticle();
            SoundManager.Instance.Shoot(dropEventRef);
            TarotSceneController.Instance.UpdateEdges();
        });
        cardLastSlot.Card = occupiedCard;
    }

    private void ReturnOccupiedCardToStartPosition(CardController occupiedCard)
    {
        occupiedCard.simpleMover.TargetPoint = startPosition;
        occupiedCard.simpleMover.RunAnimation(() =>
        {
            occupiedCard.UpdateCardSide();
            SoundManager.Instance.Shoot(dropEventRef);
            TarotSceneController.Instance.UpdateEdges();
        });
        occupiedCard.CardSlotCurrent = null;
        occupiedCard.CardSlotOld = null;
    }

    public void PlaceCardInSlot(CardSlot newCardSlot)
    {
        simpleMover.TargetPoint = newCardSlot.transform.position + Vector3.back * CardDepthOffset;
        simpleMover.RunAnimation(() =>
        {
            UpdateCardSide();
            cardSlotCurrent.ShowParticle();
            SoundManager.Instance.Shoot(dropEventRef);
            TarotSceneController.Instance.UpdateEdges();
        });
        newCardSlot.Card = this;
        CardSlotCurrent = newCardSlot;
    }

    private void ReturnCardToLastSlot()
    {
        simpleMover.TargetPoint = cardLastSlot == null ? startPosition : cardLastSlot.transform.position + Vector3.back * CardDepthOffset;
        simpleMover.RunAnimation(() =>
        {
            UpdateCardSide();
            if (cardLastSlot != null)
            {
                FixCardPosition();
                TarotSceneController.Instance.UpdateEdges();
            }
        });
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (zoomIsActive) return;
        Vector3 pointerWorldPosition = sceneCamera.ScreenToWorldPoint(eventData.position);
        transform.position = pointerWorldPosition + offset;
    }

    public void UpdateImages()
    {
        portrait.sprite = cardSettings.PortraitTexture;
    }

    public void ShowOutline(bool show = true)
    {
        if (show) SoundManager.Instance.Shoot(getEventRef);

        float targetAlpha = show ? 1f : 0f;
        float targetZoom = show ? zoomSelected : simpleScaler.StartScale;
        Color glowColor = glow.color;
        glowColor.a = targetAlpha;

        simpleScaler.StopAnimation();
        simpleScaler.TargetScale = targetZoom;
        simpleScaler.RunAnimation();

        outlineTween?.Kill();

        outlineTween = glow.DOFade(targetAlpha, transitionDuration)
            .SetLink(gameObject)
            .SetUpdate(true)
            .OnComplete(() => glow.color = glowColor);
    }
}
