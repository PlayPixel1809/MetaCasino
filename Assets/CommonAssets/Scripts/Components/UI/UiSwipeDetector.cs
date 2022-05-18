using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiSwipeDetector : MonoBehaviour
{
    public enum SwipeTypes { Left, Right, Top, Down }

    public SwipeTypes swipeType;

    public EventTrigger eventTrigger;
    public GameObject tutorial;
    public Action onSwipe;


    void OnEnable()
    {
        onSwipe = null;
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

    void Update()
    {

    }

    public void OnPointerDownDelegate(PointerEventData data)
    {
    }

    public void OnPointerDrag(PointerEventData data)
    {
        if (data.delta.x < 0 && swipeType == SwipeTypes.Left)
        {
            onSwipe?.Invoke();
        }

        if (data.delta.x > 0 && swipeType == SwipeTypes.Right)
        {
            onSwipe?.Invoke();
        }

        if (data.delta.y > 0 && swipeType == SwipeTypes.Top)
        {
            onSwipe?.Invoke();
        }

        if (data.delta.y < 0 && swipeType == SwipeTypes.Down)
        {
            onSwipe?.Invoke();
        }
    }

    public void OnPointerUpDelegate(PointerEventData data)
    {
    }

    public void InitiateDetection(Action onSwipe)
    {
        gameObject.SetActive(true);
        if (tutorial != null) { tutorial.SetActive(true); }
        this.onSwipe = onSwipe;
    }

}
