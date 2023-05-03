using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionInfo : MonoBehaviour
{
    public Action<Collider> onTriggerEnter;
    public Action<Collider> onTriggerExit;

    public Action<Collision> onCollisionEnter;
    public Action<Collision> onCollisionExit;

    private void OnTriggerEnter(Collider collider) { onTriggerEnter?.Invoke(collider); }
    private void OnTriggerExit(Collider collider) { onTriggerExit?.Invoke(collider); }
    private void OnCollisionEnter(Collision collision) { onCollisionEnter?.Invoke(collision); }
    private void OnCollisionExit(Collision collision) { onCollisionExit?.Invoke(collision); }
}
