using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public float followSpeed = .1f;
    public Transform target;

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position, followSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, followSpeed);
    }
}
