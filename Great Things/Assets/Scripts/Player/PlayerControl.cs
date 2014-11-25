using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public float moveSpeed = 0.02f;
	protected Vector3 move = Vector3.zero;
	public GeneratePaths paths;
 	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		move = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f); // Input.GetAxis("Vertical")
		move.Normalize();
		move *= moveSpeed;
		
		UpdateForMove(move);
	}

	void UpdateForMove (Vector3 move) {
		MapLocation currentLocation = paths.GetMapLocationForPosition(transform.position);
		Vector3 transformedMove = transform.TransformDirection(move);
		MapLocation exitLocation = paths.GetExitForMove(transform.position, transformedMove); 
		
		if (exitLocation == null) return;
		bool hasRotated = UpdateRotation(currentLocation, exitLocation, transformedMove);
		if (!hasRotated) {
			move = transform.right * move.x;
			transform.Translate(move, Space.World);
		}
	}
	
	bool UpdateRotation (MapLocation currentLocation, MapLocation exitLocation, Vector3 move) {
		if (currentLocation == null || exitLocation == null) return false;
		if (currentLocation.Equals(exitLocation)) return false;
		
		Vector3 oldAngle = transform.localEulerAngles;
		//Vector3 rotation = paths.GetRotationForExit(currentLocation, exitLocation);
		float newY = Mathf.Atan2((exitLocation.x - currentLocation.x), (exitLocation.z - currentLocation.z)) * 180 / Mathf.PI; 
		Vector3 rotation = new Vector3(0, newY - 90, 0);
		if (rotation.y >= 180) rotation.y -= 180;
		if (rotation.y <= -180) rotation.y += 180;
		if (!matchRotations(rotation, oldAngle)) {
			print (rotation + ":" + exitLocation  + ":" + currentLocation);
			transform.localEulerAngles = rotation;
			Vector3 position = transform.position;
			position.x = Mathf.RoundToInt(position.x);
			position.z = Mathf.RoundToInt(position.z);
			transform.position = position;
			return true;
		}
		
		return false;
	}
	
	bool matchRotations (Vector3 updated, Vector3 existing) {
		float x = Mathf.Abs(updated.x - existing.x);
		if (x > 0.01) return false;
		float y = Mathf.Abs(updated.y - existing.y);
		if (y > 0.01) return false;
		float z = Mathf.Abs(updated.z - existing.z);
		if (z > 0.01) return false;
		
		return true;
	}
}
