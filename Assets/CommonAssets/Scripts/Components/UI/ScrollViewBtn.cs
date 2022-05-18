using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ScrollViewBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject scrollView;


    public float clickDetectDuration = .25f;
    public UnityEvent onClick;

    private bool passingEvent = false;
    private float pointerDownTime;

    private Vector3 pointerDownPos;


    public void OnPointerDown(PointerEventData pointerEventData)
    {
        ExecuteEvents.Execute(scrollView, pointerEventData, ExecuteEvents.pointerDownHandler);
        pointerDownTime = Time.realtimeSinceStartup;
        pointerDownPos = pointerEventData.position;
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        ExecuteEvents.Execute(scrollView, pointerEventData, ExecuteEvents.pointerUpHandler);
        if (Vector3.Distance(pointerDownPos, pointerEventData.position) > 50) { return; }
        if (Time.realtimeSinceStartup - pointerDownTime < clickDetectDuration)
        {
            onClick.Invoke();
        }
    }


    public void OnBeginDrag(PointerEventData pointerEventData)
    {
        ExecuteEvents.Execute(scrollView, pointerEventData, ExecuteEvents.beginDragHandler);
        passingEvent = true;
    }

    public void OnDrag(PointerEventData pointerEventData)
    {
        if (passingEvent)
        {
            ExecuteEvents.Execute(scrollView, pointerEventData, ExecuteEvents.dragHandler);
        }
    }

    public void OnEndDrag(PointerEventData pointerEventData)
    {
        ExecuteEvents.Execute(scrollView, pointerEventData, ExecuteEvents.endDragHandler);
        passingEvent = false;
    }
}