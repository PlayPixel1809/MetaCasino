using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiDragDetector : MonoBehaviour
{
    public enum DragDirections { Both, Horizontal, Vertical }

    public DragDirections dragDirection;

    public EventTrigger eventTrigger;
    
    public GameObject tutorial;

    public Action<float> onDrag;

    void OnEnable()
    {
        onDrag = null;
    }


    void Start()
    {
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerDownEntry);

        EventTrigger.Entry pointerDragEntry = new EventTrigger.Entry();
        pointerDragEntry.eventID = EventTriggerType.Drag;
        pointerDragEntry.callback.AddListener((data) => { OnPointerDrag((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerDragEntry);

        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnPointerUpDelegate((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerUpEntry);
    }

    public void OnPointerDownDelegate(PointerEventData data)
    {
    }

    public void OnPointerDrag(PointerEventData data)
    {
        float dragAmount = 0;
        if (dragDirection == DragDirections.Horizontal) { dragAmount = data.delta.x; }
        if (dragDirection == DragDirections.Vertical)   { dragAmount = data.delta.y; }
        if (dragDirection == DragDirections.Both)       { dragAmount = Math.Abs(data.delta.x) + Math.Abs(data.delta.y); }

        if (dragAmount != 0)
        {
            if (tutorial != null) { tutorial.SetActive(false); }
            onDrag?.Invoke(dragAmount);
        }
    }

    public void OnPointerUpDelegate(PointerEventData data)
    {
    }

    public void InitiateDetection(Action<float> onDrag)
    {
        gameObject.SetActive(true);
        if (tutorial != null) { tutorial.SetActive(true); }
        this.onDrag = onDrag;
    }


}