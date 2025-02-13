using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using FMODUnity;
using UnityEngine.InputSystem;
using System.Linq;
using Architecture;

public class GearsSceneController : Singleton<GearsSceneController>
{
    [SerializeField] private Camera sceneCamera;
    [SerializeField] private int gearsToWin;

    [SerializeField] private GameObject contentContainer;

    [SerializeField] private ScenarioEntity currentScenario;
    private bool contentLoaded = false;

    [SerializeField] private string buzzEventParamName = "CogsActive";
    [SerializeField] private EventReference onStartEventRef;
    [SerializeField] private EventReference onWinEventRef;
    [SerializeField] private GameEvent onContentLoadedEvent;
    [SerializeField] private GameEvent onWinEvent;


    [SerializeField] private List<GearController> gearControllers = new();
    [SerializeField] private List<PinController> pinControllers = new();
    public List<PinController> PinControllers { get => pinControllers; }
    private Dictionary<GearController, List<GearController>> connections = new();
    private int gearsPlaced = 0;
    private int selectedGearIndex = 0;
    private int selectedPinIndex = 0;
    private bool isLookingForPin;

    private InputAction actionMovement;
    private InputAction actionSelect;
    private InputAction actionBack;
    private InputAction actionToInventory;

    private void OnDisable()
    {
        UnsubscribeInputActions();
    }

    void Start()
    {
        SoundManager.Instance.PlayBackgroundMusic(currentScenario.BackgroundMusic);
        SoundManager.Instance.PlayBackgroundAmbience(currentScenario.BackgroundAmbience);
    }

    void FixedUpdate()
    {
        foreach (GearController controller in gearControllers)
        {
            // Rotate the GameObject based on the rotationAxis and rotationSpeed
            controller.transform.Rotate(Vector3.forward * controller.RotationSpeed);
        }
        foreach (PinController controller in pinControllers)
        {
            // Rotate the GameObject based on the rotationAxis and rotationSpeed
            controller.transform.Rotate(Vector3.forward * controller.RotationSpeed);
        }
    }

    private void LoadContent()
    {
        if (currentScenario.ContentPrefab == null)
        {
            Debug.LogError("Contents are not set");
            return;
        }
        StartCoroutine(WaitForContent());

        GameObject content = Instantiate(currentScenario.ContentPrefab, contentContainer.transform);

        foreach (GearController gear in content.GetComponentsInChildren<GearController>())
        {
            gear.InitializeGear();
            gear.SceneCamera = sceneCamera;
            AddGear(gear);
        }

        foreach (PinController pin in content.GetComponentsInChildren<PinController>())
        {
            if (!pin.enabled) continue;

            pin.InitializePin();
            AddPin(pin);
        }

        gearControllers.Where(gear => !gear.IsDraggable).ToList().ForEach(gear => gear.ApplyGear());
        gearControllers.Where(gear => gear.RotationSpeed != 0).ToList().ForEach(gear => GearPlaced(gear));
        EnableInputs();
        contentLoaded = true;
    }

    private void EnableInputActions()
    {
        actionMovement?.Enable();
        actionSelect?.Enable();
        actionBack?.Enable();
        actionToInventory?.Enable();
    }

    private void SubscribeInputActions()
    {
        actionMovement.performed += OnDirectionInput;
        actionSelect.performed += OnSelectInput;
        actionBack.performed += OnBackInput;
        actionToInventory.performed += OnToInventoryInput;
    }

    private void UnsubscribeInputActions()
    {
        if (actionMovement != null) actionMovement.performed -= OnDirectionInput;
        if (actionSelect != null) actionSelect.performed -= OnSelectInput;
        if (actionBack != null) actionBack.performed -= OnBackInput;
        if (actionToInventory != null) actionToInventory.performed -= OnToInventoryInput;
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

        actionMovement = inputController.Actions.Gears.Movement;
        actionSelect = inputController.Actions.Gears.Select;
        actionBack = inputController.Actions.Gears.Back;
        actionToInventory = inputController.Actions.Gears.ToInventory;

        EnableInputActions();
        SubscribeInputActions();

        if (gearControllers.Count > 0) SelectGear(gearControllers.Count - 1);
        isLookingForPin = true;
        SelectPin(pinControllers.IndexOf(GetClosestPin()));
    }

    private void SelectGear(int index)
    {
        if (index < 0 || index >= gearControllers.Count) return;

        gearControllers[selectedGearIndex]?.ShowOutline(false);
        selectedGearIndex = index;
        gearControllers[selectedGearIndex].ShowOutline(true);
    }

    private void SelectPin(int index)
    {
        if (index < 0 || index >= pinControllers.Count) return;

        pinControllers[selectedPinIndex]?.ShowOutline(false);
        selectedPinIndex = index;
        bool isGearFits = IsGearFitsBySize(gearControllers[selectedGearIndex], pinControllers[selectedPinIndex]) && IsGearFitsByType(gearControllers[selectedGearIndex], pinControllers[selectedPinIndex]);
        pinControllers[selectedPinIndex].ShowOutline(true, isGearFits);
    }

    private GearController GetClosestGearInDirection(Vector2 direction)
    {
        return gearControllers
            .Where(gear => gear != gearControllers[selectedGearIndex]
                            && gear.IsDraggable
                            && (gearControllers[selectedGearIndex].BasePin == null 
                                    || gearControllers[selectedGearIndex].BasePin.BaseGear == null 
                                    || gearControllers[selectedGearIndex].BasePin.BaseGear != gear))
            .OrderBy(gear =>
            {
                Vector2 gearPosition = (Vector2)gear.transform.position;
                Vector2 toGear = gearPosition - (Vector2)gearControllers[selectedGearIndex].transform.position;
                float angle = Vector2.Angle(direction, toGear);
                float distance = toGear.magnitude;
                // Combine angle and distance into a single metric for ordering
                return angle + distance;
            })
            .FirstOrDefault();
    }

    private PinController GetClosestPinInDirection(Vector2 direction)
    {
        return pinControllers
            .Where(pin =>   pin != pinControllers[selectedPinIndex]
                            && pin.NestedGear == null 
                            && gearControllers[selectedGearIndex] != pin.BaseGear
                            && pin.Layer < 100)
            .OrderBy(pin =>
            {
                Vector2 pinPosition = (Vector2)pin.transform.position;
                Vector2 toPin = pinPosition - (Vector2)pinControllers[selectedPinIndex].transform.position;
                float angle = Vector2.Angle(direction, toPin);
                float distance = toPin.magnitude;
                // Combine angle and distance into a single metric for ordering
                return angle + distance;
            })
            .FirstOrDefault();
    }

    private PinController GetClosestPin()
    {
        return pinControllers
            .Where(pin => pin != pinControllers[selectedPinIndex]
                            && pin.NestedGear == null
                            && gearControllers[selectedGearIndex] != pin.BaseGear
                            && pin.Layer < 100)
            .OrderBy(pin => Vector3.Distance(pin.transform.position, gearControllers[selectedGearIndex].transform.position))
            .FirstOrDefault();
    }

    public void GearPlaced(bool placed)
    {
        if (placed) gearsPlaced++;
        else gearsPlaced--;

        if (gearsPlaced <= 1)
        {
            RuntimeManager.StudioSystem.setParameterByName(buzzEventParamName, 0);
        }
        else if (gearsPlaced == 2)
        {
            RuntimeManager.StudioSystem.setParameterByName(buzzEventParamName, 1);
        }
        else if (gearsPlaced == 3)
        {
            RuntimeManager.StudioSystem.setParameterByName(buzzEventParamName, 2);
        }
        else if (gearsPlaced >= 4)
        {
            RuntimeManager.StudioSystem.setParameterByName(buzzEventParamName, 3);
        }
    }

    public void CountTargetGears(bool moving)
    {
        if (!moving)
        {
            gearsToWin++;
            return;
        }
        gearsToWin--;
        if (gearsToWin > 0) return;

        SoundManager.Instance.Shoot(onWinEventRef);
        onWinEvent?.Invoke();
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

    public void AddGear(GearController gear)
    {
        if (connections.ContainsKey(gear))
        {
            Debug.LogWarning("Gear already exist");
            return;
        }
        if (gear.targetGear) gearsToWin++;
        gearControllers.Add(gear);
        connections[gear] = new List<GearController>();
    }
    
    public void RemoveGear(GearController gear)
    {
        if (!connections.ContainsKey(gear))
        {
            Debug.LogWarning("Gear not existed");
            return;
        }
        connections.Remove(gear);
        gearControllers.Remove(gear);
        foreach (GearController gc in gearControllers)
        {
            connections[gc].Remove(gear);
        }
    }

    public void AddPin(PinController pin)
    {
        if (pinControllers.Contains(pin))
        {
            Debug.LogWarning("Pin already exist");
            return;
        }

        pinControllers.Add(pin);
    }
    
    public void RemovePin(PinController pin)
    {
        if (!pinControllers.Contains(pin))
        {
            Debug.LogWarning("Pin not existed");
            return;
        }
        pinControllers.Remove(pin);
    }

    public void AddConnection(GearController gear1, GearController gear2)
    {
        if (!connections.ContainsKey(gear1) || !connections.ContainsKey(gear2))
        {
            Debug.LogWarning("gears are not added to gearControllers " + gear1 + " " + gear2);
            return;
        }

        if (connections[gear1].Contains(gear2) || connections[gear2].Contains(gear1))
        {
            Debug.LogWarning("gears are already touched");
            return;
        }

        connections[gear1].Add(gear2);
        connections[gear2].Add(gear1);
    }

    public void ClearConnection(GearController gear)
    {
        gear.DriveGear = null;
        gear.RotationSpeed = 0;

        StopConnectedGears(gear);

        if (gear.BasePin != null) 
        { 
            gear.BasePin.NestedGear = null;
            gear.BasePin = null;
        }

        if (gear.NestedPin != null && gear.NestedPin.NestedGear != null)
        {
            gear.NestedPin.NestedGear.ResetPosition();
            gear.NestedPin.NestedGear = null; 
        }

        if (connections.ContainsKey(gear)) connections[gear].Clear();

        foreach (KeyValuePair<GearController, List<GearController>> pair in connections)
        {
            List<GearController> connectedGears = pair.Value;
            connectedGears.Remove(gear);
        }
    }

    public bool AreDirectionsCompatible(GearController current, GearController target)
    {
        if (current.RotationSpeed == 0) return true;
        if (target.RotationSpeed == 0) return true;

        if (IsOnSameShaft(current.transform.position, target.transform.position))
        {
            return Mathf.Sign(current.RotationSpeed) == Mathf.Sign(target.RotationSpeed);
        }

        return Mathf.Sign(current.RotationSpeed) == (-1) * Mathf.Sign(target.RotationSpeed);
    }

    private List<GearController> FindLoops(GearController current, GearController target)
    {
        HashSet<GearController> visited = new();
        List<GearController> path = new();

        if (DFSFindLoop(current, target, visited, path))
        {
            path.Reverse(); // Reverse the path to get the correct order
            return path;
        }

        return null; // No loop found
    }

    private bool DFSFindLoop(GearController current, GearController target, HashSet<GearController> visited, List<GearController> path)
    {
        visited.Add(current);
        path.Add(current);

        foreach (GearController neighbor in connections[current])
        {
            if (neighbor == target)
            {
                path.Add(target);
                return true; // Loop found
            }

            if (!visited.Contains(neighbor) && DFSFindLoop(neighbor, target, visited, path))
            {
                return true; // Loop found
            }
        }

        path.RemoveAt(path.Count - 1); // Remove the current gear from the path
        visited.Remove(current);

        return false; // No loop found
    }

    public void RecalculateMovement()
    {
        // Clear DriveGear flags before starting the process
        foreach (GearController gc in gearControllers)
        {
            gc.DriveGear = null;
        }

        // Apply pin movement
        foreach (PinController pc in pinControllers)
        {
            if (pc.RotationSpeed == 0) continue;
            if (pc.NestedGear == null) continue;
            pc.NestedGear.RotationSpeed = pc.RotationSpeed;
            pc.NestedGear.DriveGear = pc.NestedGear;
        }

        foreach (GearController gc in gearControllers)
        {
            if (gc.BasePin == null) continue;
            if (gc.BasePin.BaseGear == null && gc.BasePin.RotationSpeed == 0) continue;
            if (gc.BasePin.BaseGear != null && gc.BasePin.BaseGear.RotationSpeed == 0) continue;

            CalculateCoupling(gc);
        }
    }

    private void CalculateCoupling(GearController engineGear)
    {
        foreach (GearController gear in connections[engineGear])
        {
            // Skip gears that have already been driven by another gear
            if (gear.DriveGear != null) continue;

            // Calculate gearRatio based on the connected gears
            float gearRatio = (float)engineGear.Settings.teeth / gear.Settings.teeth;

            // Apply the adjusted rotation speed
            if (!IsOnSameShaft(gear.transform.position, engineGear.transform.position))
            {
                gear.RotationSpeed = engineGear.RotationSpeed * gearRatio * (-1f);
            }
            else
            {
                gear.RotationSpeed = engineGear.RotationSpeed;
            }

            // Set the DriveGear to prevent redundant adjustments
            gear.DriveGear = engineGear;

            // Recursively adjust connected gears
            CalculateCoupling(gear);
        }
    }
    
    public void AdjustGearAngle(GearController gear)
    {
        if (connections[gear].Count == 0) return;

        HashSet<GearController> visited = new();
        SetAngles(gear, visited);
    }

    private void SetAngles(GearController gear, HashSet<GearController> visited)
    {
        visited.Add(gear);

        foreach (GearController gc in connections[gear])
        {
            if ((gc.BasePin && gc.BasePin.RotationSpeed != 0) || gc.DriveGear != null)
            {
                CalculateAngle(gear, gc);
                break;
            }
        }

        foreach (GearController gc in connections[gear])
        {
            if (visited.Contains(gc)) continue;
            if ((gc.BasePin && gc.BasePin.RotationSpeed != 0) || gc.DriveGear != null) continue;
            CalculateAngle(gc, gear);
            SetAngles(gc, visited);
        }
    }

    private void CalculateAngle(GearController currentGear, GearController driveGear)
    {
        if (IsOnSameShaft(currentGear.transform.position, driveGear.transform.position)) return;

        // Calculate gear ratio
        float gearRatio = (float)driveGear.Settings.teeth / (float)currentGear.Settings.teeth;

        // Calculate rotation angle
        float rotationAngle = - driveGear.transform.eulerAngles.z * gearRatio;

        // Calculate the vector between the centers of the two gears
        Vector2 gearsCenterVector = currentGear.transform.position - driveGear.transform.position;

        // Calculate the angle between the vector and the X-axis
        float vectorAngle = Mathf.Atan2(gearsCenterVector.y, gearsCenterVector.x) * Mathf.Rad2Deg ;
        vectorAngle = (vectorAngle + 360) % 360;
        rotationAngle += vectorAngle;

        float driveToothOffset = vectorAngle % GetToothAngle(driveGear.Settings.teeth);
        float currentToothOffset = (1 - driveToothOffset / (GetToothAngle(driveGear.Settings.teeth) *0.5f) ) * GetToothAngle(currentGear.Settings.teeth) / 2;

        rotationAngle -= currentToothOffset;

        // Apply rotation
        currentGear.transform.rotation = Quaternion.Euler(currentGear.transform.eulerAngles.x, currentGear.transform.eulerAngles.y, rotationAngle % GetToothAngle(currentGear.Settings.teeth));
    }

    private float GetToothAngle(float teeth)
    {
        return 360 / teeth;
    }
        
    private bool IsOnSameShaft(Vector3 goPosition1, Vector3 goPosition2)
    {
        float distance = Vector2.Distance(goPosition1, goPosition2);
        if (distance > 0.1f) return false;
        return true;
    }

    private void StopConnectedGears(GearController gear)
    {
        // Iterate through connected gears
        foreach (GearController gc in gearControllers)
        {
            // If the connected gear has the current gear as its DriveGear, stop it and its connected gears
            if (gc.DriveGear == gear)
            {
                gc.RotationSpeed = 0;
                gc.DriveGear = null;
                StopConnectedGears(gc);
            }
        }
    }

    public void OnDirectionInput(InputAction.CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        if (direction == Vector2.zero) return;

        if (isLookingForPin)
        {
            PinController closestPin = GetClosestPinInDirection(direction);
            if (closestPin != null)
            {
                SelectPin(pinControllers.IndexOf(closestPin));
            }
        }
        else
        {
            GearController closestGear = GetClosestGearInDirection(direction);
            if (closestGear != null)
            {
                SelectGear(gearControllers.IndexOf(closestGear));
            }
        }
    }

    public void OnSelectInput(InputAction.CallbackContext context)
    {
        if (isLookingForPin)
        {
            if (IsGearFitsBySize(gearControllers[selectedGearIndex], pinControllers[selectedPinIndex]) && IsGearFitsByType(gearControllers[selectedGearIndex], pinControllers[selectedPinIndex]))
            {
                isLookingForPin = false;
                pinControllers[selectedPinIndex].ShowOutline(false);
                gearControllers[selectedGearIndex].ApplyGear(pinControllers[selectedPinIndex]);
            }
        }
        else
        {
            isLookingForPin = true;
            SelectPin(pinControllers.IndexOf(GetClosestPin()));
        }
    }

    public void OnBackInput(InputAction.CallbackContext context)
    {
        if (!isLookingForPin) return;

        isLookingForPin = false;
        pinControllers[selectedPinIndex].ShowOutline(false);
    }

    public void OnToInventoryInput(InputAction.CallbackContext context)
    {
        isLookingForPin = false;
        pinControllers[selectedPinIndex].ShowOutline(false);
        gearControllers[selectedGearIndex].ResetPosition();
        RecalculateMovement();
    }

    public bool IsGearFitsBySize(GearController selectedGear, PinController targetPin)
    {
        // check if touch a pin
        Collider2D[] pinColliders = Physics2D.OverlapCircleAll(targetPin.transform.position, selectedGear.Settings.tipRadius * selectedGear.transform.localScale.x);
        foreach (Collider2D collider in pinColliders)
        {
            if (!collider.TryGetComponent<PinController>(out var pin)) continue;
            if (pin == targetPin) continue;
            if (pin == selectedGear.BasePin) continue;
            if (pin == selectedGear.NestedPin) continue;
            if (pin.Layer != targetPin.Layer) continue;

            return false;
        }

        // check if touch another gear
        Collider2D[] gearColliders = Physics2D.OverlapCircleAll(targetPin.transform.position, selectedGear.Settings.tipRadius * selectedGear.transform.localScale.x);
        foreach (Collider2D collider in gearColliders)
        {
            if (!collider.TryGetComponent<GearController>(out var gear)) continue;
            if (gear.gameObject == selectedGear.gameObject) continue;
            if (gear.Layer != targetPin.Layer) continue;

            return false;
        }

        return true;
    }

    public bool IsGearFitsByType(GearController selectedGear, PinController targetPin)
    {
        // check other gears directions
        Collider2D[] tipColliders = Physics2D.OverlapCircleAll(targetPin.transform.position, selectedGear.Settings.tipRadius * selectedGear.transform.localScale.x);
        foreach (Collider2D collider in tipColliders)
        {
            GearController gear = collider.GetComponentInParent<GearController>();

            if (gear == null) continue;
            if (gear.gameObject == selectedGear.gameObject) continue;
            if (gear.Settings.type == selectedGear.Settings.type) continue;
            if (gear.Layer != targetPin.Layer) continue;
            return false;
        }

        return true;
    }

    public bool CanGearsRotateTogether(GearController gear)
    {
        // it is okay if we have no connections
        if (connections[gear].Count == 0) return true;

        foreach (GearController gc in connections[gear])
        {
            // not ok if gears have same direction
            if (!AreDirectionsCompatible(gear, gc)) return false;

            List<GearController> loop = FindLoops(gear, gc);

            // not ok if there is a loop with even gears num
            if (loop.Count % 2 == 1) return false;
        }
        return true;
    }

    public void EnableAllPinsOutline(GearController selectedGear)
    {
        foreach (PinController pc in pinControllers)
        {
            if (pc.NestedGear != null) continue;
            if (pc.BaseGear == selectedGear) continue;
            if (pc.Layer >= 100) continue;

            if (!IsGearFitsBySize(selectedGear, pc))
            {

                pc.ShowOutline(true, false, false);
                continue;
            }

            if (!IsGearFitsByType(selectedGear, pc))
            {
                pc.ShowOutline(true, false, false);
                continue;
            }

            pc.ShowOutline(true, true, false);
        }
    }

    public void DisableAllPinsOutline()
    {
        foreach (PinController pc in pinControllers)
        {
            pc.ShowOutline(false);
        }
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
        // Clear all gear connections
        foreach (var gear in gearControllers)
        {
            ClearConnection(gear);
        }

        // Remove all gears and pins from the scene
        foreach (var gear in gearControllers)
        {
            Destroy(gear.gameObject);
        }
        gearControllers.Clear();

        foreach (var pin in pinControllers)
        {
            Destroy(pin.gameObject);
        }
        pinControllers.Clear();

        // Clear the content container
        foreach (Transform child in contentContainer.transform)
        {
            Destroy(child.gameObject);
        }

        // Reset state variables
        gearsToWin = 0;
        contentLoaded = false;
        gearsPlaced = 0;
        selectedGearIndex = 0;
        selectedPinIndex = 0;
        isLookingForPin = false;
        connections.Clear();
    }
}
