using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Image colliderImage;
    
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    public bool lookYLimitActive;
    public float lookYLimit = 60.0f;

    Vector2 rotation = Vector2.zero;

    public Transform character;
    public Transform cameraParent;

    void Start()
    {
        rotation.y = transform.eulerAngles.y;

        if (colliderImage != null) 
        {
            EventTrigger eventTrigger = colliderImage.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDragEntry = new EventTrigger.Entry();
            pointerDragEntry.eventID = EventTriggerType.Drag;
            pointerDragEntry.callback.AddListener((data) => { OnPointerDrag((PointerEventData)data); });
            eventTrigger.triggers.Add(pointerDragEntry);
        }

        
    }

    void Update()
    {
        if (colliderImage == null ) 
        {
            rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
            rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;

            rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
            if (lookYLimitActive) { rotation.y = Mathf.Clamp(rotation.y, -lookYLimit, lookYLimit); }

            cameraParent.localRotation = Quaternion.Euler(rotation.x, cameraParent.localRotation.y, 0);
            character.eulerAngles = new Vector2(character.eulerAngles.x, rotation.y);
        }
    }

    public void OnPointerDrag(PointerEventData data)
    {
        rotation.y += data.delta.x * Time.deltaTime * lookSpeed;
        rotation.x += -data.delta.y * Time.deltaTime * lookSpeed;

        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        if (lookYLimitActive) { rotation.y = Mathf.Clamp(rotation.y, -lookYLimit, lookYLimit); }

        cameraParent.localRotation = Quaternion.Euler(rotation.x, cameraParent.localRotation.y, 0);
        character.eulerAngles = new Vector2(character.eulerAngles.x, rotation.y);
    }
    
}