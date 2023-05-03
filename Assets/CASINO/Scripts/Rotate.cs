using UnityEngine;

public class Rotate : MonoBehaviour {
	
		public float x=0;
		public float y=0;
		public float z=0;
	
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.right * Time.deltaTime*-55*x);
		transform.Rotate(Vector3.up * Time.deltaTime*-55*y);
		transform.Rotate(Vector3.forward * Time.deltaTime*-55*z);
	}

}