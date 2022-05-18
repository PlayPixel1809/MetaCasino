using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Translate : MonoBehaviour
{
	public float xAxisSpeed;
	public float yAxisSpeed;
	public float zAxisSpeed;

	
	void Update()
	{
		if (Input.GetKey("z")) { transform.Translate(xAxisSpeed * Time.deltaTime, yAxisSpeed * Time.deltaTime, zAxisSpeed * Time.deltaTime); }
	}
}
