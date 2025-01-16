using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CursorState
{
    Default, Item, Combine,
}

[CreateAssetMenu(fileName = "NewCursorEntity", menuName = "Cursor Entity")]
public class CursorEntity : ScriptableObject
{
    [SerializeField] public CursorState cursorType;
    [SerializeField] public Texture2D cursor;
    [SerializeField] public Texture2D cursorPressed;
}

public class CursorController : MonoBehaviour
{
    [SerializeField] List<CursorEntity> cursorList = new();

    [SerializeField] Vector2 offset;
    private CursorState state;
    public CursorState State { get => state; }

    // Start is called before the first frame update
    void Start()
    {
        CursorEntity defaultCursorEntity = cursorList.Find(ce => ce.cursorType == CursorState.Default);
        if (defaultCursorEntity == null) return;
        Cursor.SetCursor(defaultCursorEntity.cursor, offset, CursorMode.ForceSoftware);
    }

    void Update()
    {
        // Check for mouse button down
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            SetCursor(true);
        }

        // Check for mouse button up
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            SetCursor(false);
        }
    }

    private void SetCursor(bool pressed)
    {
        CursorEntity ce = cursorList.Find(ce => ce.cursorType == state);
        if (ce == null) return;

        if (pressed) Cursor.SetCursor(ce.cursorPressed, offset, CursorMode.ForceSoftware);
        else Cursor.SetCursor(ce.cursor, offset, CursorMode.ForceSoftware);
    }

    public void SetDefaultState()
    {
        state = CursorState.Default;
        SetCursor(false);
    }

    public void SetState(int newState)
    {
        state = (CursorState)newState;
        SetCursor(false);
    }
}
