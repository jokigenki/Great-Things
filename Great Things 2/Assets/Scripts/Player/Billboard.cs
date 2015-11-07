using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		Vector3 direction = transform.position - Camera.main.transform.position;
		transform.rotation = Quaternion.LookRotation(direction);
	}
}
