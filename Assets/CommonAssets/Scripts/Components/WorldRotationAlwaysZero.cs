using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldRotationAlwaysZero : MonoBehaviour {

    public bool xRotation;
    public bool yRotation;
    public bool zRotation;

    void Start ()
    {
		
	}
	
	void Update ()
    {
        if (xRotation) { transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z); }
        if (yRotation) { transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z); }
        if (zRotation) { transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0); }
    }
}
