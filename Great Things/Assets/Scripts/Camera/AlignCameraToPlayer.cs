using UnityEngine;
using System.Collections;

public class AlignCameraToPlayer : MonoBehaviour {

	public float distance;
	[HideInInspector]
	public GameObject player;
	
	Vector3 targetPosition;
	Vector3 currentPosition;
	float targetRotation;
	Vector3 currentRotation;
	Vector3 targetVelocity;
	Vector3 currentVelocity;
	bool firstPositionSet;
	
	// Use this for initialization
	void Start () {
		targetPosition = Vector3.zero;
		targetRotation = 0;
		firstPositionSet = false;		
		currentVelocity = Vector3.zero;
		targetVelocity = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
		if (player == null) return;
		
		float playerRotation = player.transform.eulerAngles.y;
		float x = distance * Mathf.Sin(playerRotation * Mathf.PI / 180);
		float z = distance * Mathf.Cos(playerRotation * Mathf.PI / 180);
		
		targetPosition.x = player.transform.position.x - x;
		targetPosition.z = player.transform.position.z - z;
		targetRotation = playerRotation;
		
		if (!firstPositionSet) {
			currentPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
			firstPositionSet = true;
			currentRotation = new Vector3(0, targetRotation, 0);
		}
		
		// calculate distance to target
		// calcaulate
		float xOff = targetPosition.x - currentPosition.x;
		float zOff = targetPosition.z - currentPosition.z;
		float rOff = targetRotation - currentRotation.y;
		if (rOff > 180) rOff -=360;
		else if (rOff < -180) rOff += 360;
		
		
		float step = 4000;
		float min = 1 / step;
		if (xOff < min && xOff > -min) {
			currentPosition.x = targetPosition.x;
			currentVelocity.x = 0;
		} else {
			currentVelocity.x = currentVelocity.x + (xOff / step);
			if (currentVelocity.x > 0.1f) currentVelocity.x = 0.1f;
			else if (currentVelocity.x < -0.1f) currentVelocity.x = -0.1f;
			currentPosition.x += currentVelocity.x;
		}
		if (zOff < min && zOff > -min) {
			currentPosition.z = targetPosition.z;
			currentVelocity.z = 0;
		} else {
			currentVelocity.z = currentVelocity.z + (zOff / step);
			if (currentVelocity.z > 0.1f) currentVelocity.z = 0.1f;
			else if (currentVelocity.z < -0.1f) currentVelocity.z = -0.1f;
			currentPosition.z += currentVelocity.z;
		}
		if (rOff < min && rOff > -min) currentRotation.y = targetRotation;
		else currentRotation.y = currentRotation.y + (rOff / step);
		
		transform.position = currentPosition;
		transform.eulerAngles = currentRotation;
	}
}
