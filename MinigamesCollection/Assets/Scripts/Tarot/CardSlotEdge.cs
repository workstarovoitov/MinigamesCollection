using UnityEngine;
using FMODUnity;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class CardSlotEdge : MonoBehaviour
{
    [Header("Line end points")]
    [SerializeField] private CardSlot fromSlot;
    private CardSuit fromSuit;
    public CardSlot FromSlot { get => fromSlot; }
    [SerializeField] private CardSlot toSlot;
    public CardSlot ToSlot { get => toSlot; }
    private CardSuit toSuit;

    [Header("Material")]
    [SerializeField] private Material defaultEdgeMaterial;
    [SerializeField] private Color baseColor;
    [SerializeField] private Vector2 colorIntensity = new Vector2(0.75f, 20);

    [Header("SFX")]
    [SerializeField] private EventReference fadeInEventRef;
    [SerializeField] private EventReference fadeOutEventRef;

    private Material edgeMaterial;
    private LineRenderer lineRenderer;
    private bool wasConnected = false;
    private bool isConnected = false;
    public bool IsConnected => isConnected;

    private void Awake()
    {
        InitializeComponents();
        if (fromSlot == null || toSlot == null) return;
        UpdateLineRendererPositions();
        CalculateSpeed();
    }

    private void InitializeComponents()
    {
        edgeMaterial = Instantiate(defaultEdgeMaterial);
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = edgeMaterial;
        SetEdgeMaterialColors();
    }

    private void SetEdgeMaterialColors()
    {
        edgeMaterial.SetColor("_Color", baseColor * colorIntensity.y);
        edgeMaterial.SetColor("_ColorBase", baseColor * colorIntensity.x);
        edgeMaterial.SetFloat("_Width", -0.01f);
    }

    private void UpdateLine()
    {
        if (fromSlot == null || toSlot == null) return;

        wasConnected = isConnected;
        isConnected = IsConnectedCorrect();

        if (isConnected && !wasConnected)
        {
            AnimateLineWidth(1f);
            SoundManager.Instance.Shoot(fadeInEventRef);
        }
        else if (!isConnected && wasConnected)
        {
            AnimateLineWidth(-0.01f);
            SoundManager.Instance.Shoot(fadeOutEventRef);
        }
    }

    private void AnimateLineWidth(float width)
    {
        var simpleLineWidth = GetComponent<SimpleLineWidth>();
        simpleLineWidth.SetParams(width, edgeMaterial);
        simpleLineWidth.RunAnimation();
    }

    private void OnDrawGizmos()
    {
        if (fromSlot == null || toSlot == null) return;
        UpdateLineRendererPositions();
        CalculateSpeed();
    }

    private void UpdateLineRendererPositions()
    {
        lineRenderer.SetPosition(0, fromSlot.transform.position);
        lineRenderer.SetPosition(1, toSlot.transform.position);
    }

    private void CalculateSpeed()
    {
        if (fromSlot == null || toSlot == null) return;

        Vector3 direction = (toSlot.transform.position - fromSlot.transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Vector2 speed = defaultEdgeMaterial.GetVector("_Speed");
        Vector2 speedCalculated = RotateVector(speed, angle);
        lineRenderer.sharedMaterial.SetVector("_Speed", speedCalculated);
    }

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float cosAngle = Mathf.Cos(angle * Mathf.Deg2Rad);
        float sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad);
        return new Vector2(
            vector.x * cosAngle - vector.y * sinAngle,
            vector.x * sinAngle + vector.y * cosAngle
        );
    }

    public void UpdateEdge()
    {
        fromSuit = fromSlot == null || fromSlot.IsFree ? CardSuit.Empty : fromSlot.Card.CardSettings.Suit;
        toSuit = toSlot == null || toSlot.IsFree ? CardSuit.Empty : toSlot.Card.CardSettings.Suit;
        UpdateLine();
    }

    private bool IsConnectedCorrect()
    {
        return (fromSuit == CardSuit.Feather && (toSuit == CardSuit.Flock || toSuit == CardSuit.Sword)) ||
               (fromSuit == CardSuit.Sword && (toSuit == CardSuit.Force || toSuit == CardSuit.Feather)) ||
               (fromSuit == CardSuit.Stranger && (toSuit == CardSuit.Flock || toSuit == CardSuit.Force)) ||
               (fromSuit == CardSuit.Force && (toSuit == CardSuit.Stranger || toSuit == CardSuit.Sword)) ||
               (fromSuit == CardSuit.Flock && (toSuit == CardSuit.Feather || toSuit == CardSuit.Stranger));
    }
}
