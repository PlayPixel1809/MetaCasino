using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiHoldDetector : MonoBehaviour
{
    public EventTrigger eventTrigger;
    public GameObject tutorial;
    public Action onHold;
    public Action onUnHold;

    private bool hold;
    void OnEnable()
    {
        hold = false;
        onHold = null;
        onUnHold = null;
    }

    void Start()
    {
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => { OnPointerDownDelegate((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerDownEntry);


        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => { OnPointerUpDelegate((PointerEventData)data); });
        eventTrigger.triggers.Add(pointerUpEntry);
    }

    void Update()
    {
        if (hold) { onHold?.Invoke(); }
        
    }

    public void OnPointerDownDelegate(PointerEventData data)
    {
        if (tutorial != null) { tutorial.SetActive(false); }
        hold = true;
    }


    public void OnPointerUpDelegate(PointerEventData data)
    {
        hold = false;
        onUnHold?.Invoke();
    }

    public void InitiateDetection(Action onHold, Action onUnHold = null)
    {
        gameObject.SetActive(true);
        if (tutorial != null) { tutorial.SetActive(true); }
        this.onHold = onHold;
        this.onUnHold = onUnHold;
    }

}
