using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public enum VirtualDPadDirection { Both, Horizontal, Vertical }

public class VirtualDPad : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform centerArea = null;
    [SerializeField] private RectTransform handle = null;
    [SerializeField] private VirtualDPadDirection direction = VirtualDPadDirection.Both;
    [InputControl(layout = "Vector2")]
    [SerializeField] private string dPadControlPath;
    [SerializeField] private float movementRange = 10f;
    [SerializeField] private float moveThreshold = 0f;
    [SerializeField] private float uiMovementRange = 10f;
    [SerializeField] private bool forceIntValue = true;

    private Vector3 startPos;

    protected override string controlPathInternal
    {
        get => dPadControlPath;
        set => dPadControlPath = value;
    }

    private void Awake()
    {
        if (centerArea == null)
            centerArea = GetComponent<RectTransform>();

        Vector2 center = new Vector2(0.5f, 0.5f);
        centerArea.pivot = center;
        handle.anchorMin = center;
        handle.anchorMax = center;
        handle.pivot = center;
        handle.anchoredPosition = Vector2.zero;
    }

    private void Start()
    {
        startPos = handle.anchoredPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        OnDrag(eventData);
    }

public void OnDrag(PointerEventData eventData)
{
    if (eventData == null)
        throw new System.ArgumentNullException(nameof(eventData));

    RectTransformUtility.ScreenPointToLocalPointInRectangle(handle.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
    Vector2 delta = position;

    // Apply direction constraints
    if (direction == VirtualDPadDirection.Horizontal) delta.y = 0;
    else if (direction == VirtualDPadDirection.Vertical) delta.x = 0;

    Vector2 buttonDelta = Vector2.ClampMagnitude(delta, uiMovementRange);
    handle.anchoredPosition = startPos + (Vector3)buttonDelta;

    Vector2 newPos = SanitizePosition(delta);
    Debug.Log($"Dragging: {newPos}");
    SendValueToControl(newPos);
}


    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = startPos;
        SendValueToControl(Vector2.zero);
    }

private Vector2 SanitizePosition(Vector2 pos)
{
    pos = Vector2.ClampMagnitude(pos, movementRange);

    float minMovementRange = this.moveThreshold > movementRange ? movementRange : this.moveThreshold;

    // Ignore X movement
    pos.x = 0;

    // Determine the Y value based on the position
    if (pos.y > 0.5f) 
        pos.y = 2;    // Strong upward movement
    else if (pos.y > 0.1f) 
        pos.y = 1.5f; // Moderate upward movement
    else if (pos.y < -0.5f) 
        pos.y = -2;   // Strong downward movement
    else if (pos.y < -0.1f) 
        pos.y = -1.5f; // Moderate downward movement
    else 
        pos.y = 0;    // Neutral position

    return pos;
}




}
