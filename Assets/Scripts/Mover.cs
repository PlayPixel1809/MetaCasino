using UnityEngine;
using System.Collections;

public class Mover : MonoBehaviour {
	
	Transform selfTransform;
	public Transform directionTransform; 
	public float speed=5;
	public bool fly; 
	Vector3 forwardDirection,sidewaysDirection; 

	void Start(){
		selfTransform = transform;
		if (directionTransform == null) directionTransform = transform; 
	}

	void Update () {
		/*forwardDirection = directionTransform.forward -  new Vector3(0,directionTransform.forward.y,0);
		sidewaysDirection = directionTransform.right; 

		selfTransform.position = selfTransform.position + ((forwardDirection + sidewaysDirection) * speed * Time.deltaTime);*/

		if (Input.GetMouseButtonUp(0)) 
		{
			SetSpeed();
		}

		Vector3 recPos = transform.position;
		transform.Translate(directionTransform.forward * speed * Time.deltaTime);
		transform.position = new Vector3(transform.position.x, recPos.y, transform.position.z);

	}

	void SetSpeed()
	{
		if (speed == 0) { speed = 2; return; }
		if (speed == 2) { speed = 5; return; }
		if (speed == 5) { speed = 0; return; }
	}

}
