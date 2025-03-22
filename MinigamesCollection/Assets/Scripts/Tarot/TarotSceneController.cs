using Architecture;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using UnityEngine.InputSystem;
using System.Linq;
using System;

public class TarotSceneController : Singleton<TarotSceneController>
{
    [SerializeField] private Camera sceneCamera;

    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private GameObject contentContainer;

    [SerializeField] private ScenarioEntity currentScenario;
    private bool contentLoaded = false;

    [SerializeField] private EventReference onStartEventRef;
    [SerializeField] private EventReference onWinEventRef;
    [SerializeField] private GameEvent onContentLoadedEvent;
    [SerializeField] private GameEvent onWinEvent;

    private List<CardSlotEdge> cardSlotEdges = new();
    private List<CardController> cardControllers = new();
    private List<CardSlot> cardSlots = new();
    private int selectedCardIndex = 0;
    private int selectedSlotIndex = 0;
    private bool isLookingForSlot = false;

    [SerializeField] private List<CardSettings> AllCards;

    [SerializeField] private List<CardSuit> winSuits = new();
    [SerializeField] private List<CardSettings> winCards = new();

    private const float CardDepthOffset = 0.01f;

    private InputAction actionMovement;
    private InputAction actionSelect;
    private InputAction actionBack;
    private InputAction actionZoom;

    //private List<CardController> availableCards;
    // Create a graph representation of the connections
    Dictionary<CardSlot, List<CardSlot>> graph = new();
    private Transform cardsSpawnPosition;

    private void OnDisable()
    {
        UnsubscribeInputActions();
    }

    private void Start()
    {
        SoundManager.Instance.PlayBackgroundMusic(currentScenario.BackgroundMusic);
        SoundManager.Instance.PlayBackgroundAmbience(currentScenario.BackgroundAmbience);
    }

    public void EnableInputs()
    {
        var inputController = ServiceLocator.Get<InputController>();
        if (inputController == null)
        {
            Debug.LogError("InputController is not set");
            return;
        }

        if (inputController.CurrentInputDevice != InputDeviceType.Gamepad) return;

        actionMovement = inputController.Actions.Tarot.Movement;
        actionSelect = inputController.Actions.Tarot.Select;
        actionBack = inputController.Actions.Tarot.Back;
        actionZoom = inputController.Actions.Tarot.Zoom;

        EnableInputActions();
        SubscribeInputActions();

        if (cardControllers.Count > 0) SelectCard(cardControllers.Count - 1);
        isLookingForSlot = true;
        SelectSlot(cardSlots.IndexOf(GetClosestSlot()));
    }

    private void InstantiateCards()
    {
        GenerateWinCardSuits();
        GenerateWinCards();

        int layer = 0;
        foreach (CardSettings cardSettings in winCards)
        {
            GameObject newCard = Instantiate(cardPrefab, contentContainer.transform);
            newCard.name = cardSettings.CardName;
            var cardController = newCard.GetComponent<CardController>();
            cardController.UpdateCardSide(++layer, true);
            cardController.CardSettings = cardSettings;
            cardController.StartPosition = cardsSpawnPosition.position + Vector3.back * CardDepthOffset * layer;
            newCard.transform.position = cardController.StartPosition;
            cardController.SceneCamera = sceneCamera;
            cardControllers.Add(cardController);
        }
    }

    private void GenerateWinCardSuits()
    {
        int restartCount = 0;
        bool containsEmpty;

        do
        {
            // Dictionary to store the required suits for each slot
            Dictionary<CardSlot, CardSuit> requiredSuits = new();
            graph.Clear();

            // Initialize the graph
            foreach (var slot in cardSlots)
            {
                graph[slot] = new List<CardSlot>();
            }

            // Populate the graph with edges
            foreach (var edge in cardSlotEdges)
            {
                graph[edge.FromSlot].Add(edge.ToSlot);
                graph[edge.ToSlot].Add(edge.FromSlot);
            }

            // Traverse the graph to determine the required suits
            foreach (var slot in cardSlots)
            {
                if (!requiredSuits.ContainsKey(slot))
                {
                    HashSet<CardSlot> visited = new();
                    AssignSuits(slot, requiredSuits, GetRandomSuit(), visited);
                }
            }

            // Fill the winSuits list based on the required suits
            winSuits = requiredSuits.Values.ToList();

            // Check if winSuits contains CardSuit.Empty
            containsEmpty = winSuits.Contains(CardSuit.Empty) || winSuits.Count < cardSlots.Count;
           
            if (containsEmpty)
            {
                restartCount++;
                Debug.Log($"Restarting GenerateWinCardSuits. Restart count: {restartCount}");
            }

        } while (containsEmpty);

        Debug.Log($"GenerateWinCardSuits completed with {restartCount} restarts.");
    }

    private CardSuit GetRandomSuit()
    {
        Array suits = Enum.GetValues(typeof(CardSuit));
        System.Random random = new System.Random();
        CardSuit randomSuit;
        randomSuit = (CardSuit)suits.GetValue(random.Next(suits.Length));
        return randomSuit;
    }

    private void AssignSuits(CardSlot slot, Dictionary<CardSlot, CardSuit> requiredSuits, CardSuit suit, HashSet<CardSlot> visited)
    {
        if (requiredSuits.ContainsKey(slot))
        {
            return;
        }

        requiredSuits[slot] = suit;
        visited.Add(slot);

        foreach (var neighbor in graph[slot])
        {
            if (!requiredSuits.ContainsKey(neighbor))
            {
                // Find a suit that satisfies the connection rule
                CardSuit nextSuit = FindValidSuit(suit, neighbor, requiredSuits);
                AssignSuits(neighbor, requiredSuits, nextSuit, visited);
            }
            else
            {
                // Check if the existing suit assignment is valid
                if (!IsConnectionValid(suit, requiredSuits[neighbor]))
                {
                    // If not valid, backtrack and try a different suit
                    requiredSuits.Remove(slot);
                    visited.Remove(slot);
                    return;
                }
            }
        }
    }

    private CardSuit FindValidSuit(CardSuit currentSuit, CardSlot neighbor, Dictionary<CardSlot, CardSuit> requiredSuits)
    {
        List<CardSuit> suits = Enum.GetValues(typeof(CardSuit)).Cast<CardSuit>().ToList();
        Shuffle(suits);

        foreach (CardSuit suit in suits)
        {
            // Temporarily assign the suit to the neighbor
            requiredSuits[neighbor] = suit;

            // Check if the connection is valid
            if (IsConnectionValid(currentSuit, suit))
            {
                requiredSuits.Remove(neighbor);
                return suit;
            }

            requiredSuits.Remove(neighbor);
        }

        // Default to the first suit if no valid suit is found
        return suits.First();
    }

    private void Shuffle<T>(IList<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private bool IsConnectionValid(CardSuit fromSuit, CardSuit toSuit)
    {
        return (fromSuit == CardSuit.Feather && (toSuit == CardSuit.Flock || toSuit == CardSuit.Sword)) ||
               (fromSuit == CardSuit.Sword && (toSuit == CardSuit.Force || toSuit == CardSuit.Feather)) ||
               (fromSuit == CardSuit.Stranger && (toSuit == CardSuit.Flock || toSuit == CardSuit.Force)) ||
               (fromSuit == CardSuit.Force && (toSuit == CardSuit.Stranger || toSuit == CardSuit.Sword)) ||
               (fromSuit == CardSuit.Flock && (toSuit == CardSuit.Feather || toSuit == CardSuit.Stranger));
    }

    private void GenerateWinCards()
    {
        winCards.Clear();
        List<CardSettings> availableCards = new List<CardSettings>(AllCards);
        Shuffle(availableCards);

        foreach (var suit in winSuits)
        {
            var matchingCards = availableCards.Where(card => card.Suit == suit).ToList();
            if (matchingCards.Count > 0)
            {
                var selectedCard = matchingCards[UnityEngine.Random.Range(0, matchingCards.Count)];
                winCards.Add(selectedCard);
                availableCards.Remove(selectedCard);
            }
            else
            {
                Debug.LogWarning($"No matching card found for suit: {suit}");
            }
        }
    }

    private void SelectCard(int index)
    {
        if (index < 0 || index >= cardControllers.Count) return;

        cardControllers[selectedCardIndex]?.ShowOutline(false);
        selectedCardIndex = index;
        cardControllers[selectedCardIndex].ShowOutline(true);
        cardControllers[selectedCardIndex].UpdateCardSide();
    }

    private void SelectSlot(int index)
    {
        if (index < 0 || index >= cardSlots.Count) return;

        cardSlots[selectedSlotIndex]?.ShowOutline(false);
        selectedSlotIndex = index;
        cardSlots[selectedSlotIndex].ShowOutline(true);
    }

    public void UpdateEdges()
    {
        if (!enabled) return;

        foreach (CardSlotEdge cardSlotEdge in cardSlotEdges)
        {
            cardSlotEdge.UpdateEdge();
        }

        if (cardSlotEdges.All(edge => edge.IsConnected))
        {
            foreach(var card in cardControllers)
            {
                card.enabled = false;
            }

            enabled = false;
            SoundManager.Instance.Shoot(onWinEventRef);
            onWinEvent?.Invoke();
        }
    }

    IEnumerator WaitForContent()
    {
        while (!contentLoaded)
        {
            yield return null;
        }
        yield return new WaitForEndOfFrame();
        onContentLoadedEvent?.Invoke();
    }

    public void OnDirectionInput(InputAction.CallbackContext context)
    {
        if (cardControllers[selectedCardIndex].ZoomIsActive) return;

        Vector2 direction = context.ReadValue<Vector2>();
        if (direction == Vector2.zero) return;

        if (isLookingForSlot)
        {
            CardSlot closestSlot = GetClosestSlotInDirection(direction);
            if (closestSlot != null)
            {
                SelectSlot(cardSlots.IndexOf(closestSlot));
            }
        }
        else
        {
            CardController closestCard = GetClosestCardInDirection(direction);
            if (closestCard != null)
            {
                SelectCard(cardControllers.IndexOf(closestCard));
            }
        }
    }

    private CardSlot GetClosestSlotInDirection(Vector2 direction)
    {
        return cardSlots
            .Where(slot => slot != cardSlots[selectedSlotIndex] && slot.Card != cardControllers[selectedCardIndex])
            .OrderBy(slot => Vector2.Angle(direction, (Vector2)(slot.transform.position - cardSlots[selectedSlotIndex].transform.position)))
            .ThenBy(slot => Vector3.Distance(slot.transform.position, cardSlots[selectedSlotIndex].transform.position))
            .FirstOrDefault();
    }

    private CardSlot GetClosestSlot()
    {
        return cardSlots
            .Where(slot => slot.Card != cardControllers[selectedCardIndex])
            .OrderBy(slot => Vector3.Distance(slot.transform.position, cardControllers[selectedCardIndex].transform.position))
            .FirstOrDefault();
    }

    private CardController GetClosestCardInDirection(Vector2 direction)
    {
        return cardControllers
            .Where(card => card != cardControllers[selectedCardIndex])
            .OrderBy(card => Vector2.Angle(direction, (Vector2)(card.transform.position - cardControllers[selectedCardIndex].transform.position)))
            .ThenBy(card => Vector3.Distance(card.transform.position, cardControllers[selectedCardIndex].transform.position))
            .FirstOrDefault();
    }

    public void OnSelectInput(InputAction.CallbackContext context)
    {
        if (cardControllers[selectedCardIndex].ZoomIsActive)
        {
            cardControllers[selectedCardIndex].ZoomCard(false);
            return;
        }

        if (isLookingForSlot)
        {
            isLookingForSlot = false;
            cardControllers[selectedCardIndex].OnPointerDown(null);
            cardSlots[selectedSlotIndex].ShowOutline(false);

            if (cardSlots[selectedSlotIndex].IsFree)
            {
                cardControllers[selectedCardIndex].PlaceCardInSlot(cardSlots[selectedSlotIndex]);
            }
            else
            {
                cardControllers[selectedCardIndex].HandleOccupiedSlot(cardSlots[selectedSlotIndex]);
            }
        }
        else
        {
            isLookingForSlot = true;
            SelectSlot(cardSlots.IndexOf(GetClosestSlot()));
        }
    }

    public void OnBackInput(InputAction.CallbackContext context)
    {
        if (cardControllers[selectedCardIndex].ZoomIsActive)
        {
            cardControllers[selectedCardIndex].ZoomCard(false);
            return;
        }

        if (isLookingForSlot)
        {
            isLookingForSlot = false;
            cardSlots[selectedSlotIndex].ShowOutline(false);
        }
    }

    public void ZoomCard(InputAction.CallbackContext context)
    {
        cardControllers[selectedCardIndex].ZoomCard(!cardControllers[selectedCardIndex].ZoomIsActive);
    }

    private void EnableInputActions()
    {
        actionMovement?.Enable();
        actionSelect?.Enable();
        actionBack?.Enable();
        actionZoom?.Enable();
    }

    private void SubscribeInputActions()
    {
        actionMovement.performed += OnDirectionInput;
        actionSelect.performed += OnSelectInput;
        actionBack.performed += OnBackInput;
        actionZoom.performed += ZoomCard;
    }

    private void UnsubscribeInputActions()
    {
        if (actionMovement != null) actionMovement.performed -= OnDirectionInput;
        if (actionSelect != null) actionSelect.performed -= OnSelectInput;
        if (actionBack != null) actionBack.performed -= OnBackInput;
        if (actionZoom != null) actionZoom.performed -= ZoomCard;
    }

    public void StartGame(ScenarioEntity newScenario)
    {
        SoundManager.Instance.Shoot(onStartEventRef);
        currentScenario = newScenario;

        if (currentScenario == null)
        {
            Debug.LogError("Current scenario is not set");
        }

        LoadContent();
    }

    public void StartGame()
    {
        SoundManager.Instance.Shoot(onStartEventRef);
        currentScenario = GameManager.Instance.CurrentScenario;

        if (currentScenario == null)
        {
            Debug.LogError("Current scenario is not set");
        }

        LoadContent();
    }

    public void ClearContent()
    {
        cardControllers.Clear();
        cardSlots.Clear();
        cardSlotEdges.Clear();
        graph.Clear();

        // Clear the content container
        foreach (Transform child in contentContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Reset state variables
        contentLoaded = false;
        selectedCardIndex = 0;
        selectedSlotIndex = 0;
        isLookingForSlot = false;
    }

    private void LoadContent()
    {
        if (currentScenario.ContentPrefab == null)
        {
            Debug.LogError("Contents are not set");
            return;
        }
        
        ClearContent();

        StartCoroutine(WaitForContent());

        GameObject content = Instantiate(currentScenario.ContentPrefab, contentContainer.transform);
        cardsSpawnPosition = content.transform.Find("CardsSpawnPosition");
        cardSlotEdges.AddRange(content.GetComponentsInChildren<CardSlotEdge>());
        cardSlots.AddRange(content.GetComponentsInChildren<CardSlot>());

        InstantiateCards();
        UpdateEdges();
        EnableInputs();
        contentLoaded = true;
    }
}
