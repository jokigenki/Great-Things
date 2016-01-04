using UnityEngine;
using System.Collections;

public class SmoothCameraTracker : MonoBehaviour {

	public GameObject target;
	public GameObject followCamera;
	public string blockingName;
	public float easeSpeed = 0.01f;
	public float targetYForRotationOffset = 0.3f;
	public float maxRotationX = 2f;
	public Locale locale;
	Vector3 targetPosition;
	
	Vector3 heightCheck;
	Vector3 heightDirectionCheck;
	float targetRotation;
	Vector3 currentRotation;
	float distance = 5;
	
	//Vector3 followerVelocity;
	Vector3 pastCameraPosition, pastTargetPosition;
	
	PlayerControl control;
	
	bool firstPositionSet;
	
	void Start()
	{
		heightCheck = Vector3.zero;
		heightDirectionCheck = Vector3.zero;
		targetRotation = 0;
		control = target.GetComponent<PlayerControl>();
	}
	
	void Update()
	{	
		if (locale == null) return;
		
		Transform targetTransform = target.transform;
		Transform camTransform = followCamera.transform;
		
		float playerRotation = control != null ? control.yRotation : targetTransform.eulerAngles.y;
		float x = distance * Mathf.Sin(playerRotation * Mathf.PI / 180);
		float z = distance * Mathf.Cos(playerRotation * Mathf.PI / 180);
		
		targetPosition.x = targetTransform.position.x - x;
		targetPosition.z = targetTransform.position.z - z;
		targetRotation = playerRotation;
		
		targetPosition.y = AdjustCameraYForTerrain(targetTransform.position, camTransform.position);
		
		if (!firstPositionSet) {
			pastCameraPosition = new Vector3(targetPosition.x, targetPosition.y, targetPosition.z);
			//pastCameraRotation = targetRotation;
			//pastTargetRotation = targetRotation;
			pastTargetPosition = targetPosition;
			firstPositionSet = true;
			currentRotation = new Vector3(0, targetRotation, 0);
			followCamera.transform.position = pastCameraPosition;
		}

		// move camera //
		Vector3 lerped = SuperSmoothLerp( pastCameraPosition, pastTargetPosition, targetPosition, Time.time, easeSpeed );
		//lerped.y = pastCameraPosition.y;
		pastCameraPosition = camTransform.position;
		pastTargetPosition = targetPosition;
		followCamera.transform.position = lerped;
		
		// rotate camera
		//float lerpedRotation = SuperSmoothLerpValue(pastCameraRotation, pastTargetRotation, targetRotation, Time.time, 0.0016f );
		float lookAtRotationX = GetLookAtRotationX(targetTransform, camTransform, x, z);
		float lookAtRotationY = Mathf.Atan2(targetTransform.position.x - camTransform.position.x, targetTransform.position.z - camTransform.position.z) / Mathf.PI * 180;
		//if (lookAtRotationX > maxRotationX) lookAtRotationX = maxRotationX;
		currentRotation.x = -lookAtRotationX;
		currentRotation.y = lookAtRotationY;
		//pastCameraRotation = currentRotation.y;
		//pastTargetRotation = targetRotation;
		followCamera.transform.localEulerAngles = currentRotation;
	}
	
	float GetLookAtRotationX (Transform targetTransform, Transform camTransform, float x, float z) {
		float v = z;
		if (control.yRotation == 90) v = x;
		else if (control.yRotation == 270) v = -x;
		else if (control.yRotation == 180) v = -z;
		return Mathf.Atan2((targetTransform.position.y + targetYForRotationOffset) - camTransform.position.y, v) / Mathf.PI * 180;
	}
	
	float AdjustCameraYForTerrain (Vector3 targetPosition, Vector3 cameraPosition) {
		
		MapLocation targetLocation = locale.map.GetMapLocationForPosition(targetPosition);
		MapLocation cameraLocation = locale.map.GetMapLocationForPosition(cameraPosition);
		if (targetLocation == null || cameraLocation == null) return 0.5f;
		
		heightDirectionCheck.x = targetLocation.x - cameraLocation.x;
		heightDirectionCheck.z = targetLocation.z - cameraLocation.z;
		int c = Mathf.CeilToInt(heightDirectionCheck.magnitude);
		heightDirectionCheck.Normalize();
		heightCheck = CopyVector(cameraPosition, new Vector3());
		float highest = -1000f;
		heightCheck.y += 10f;
		Vector3 a = new Vector3();
		for (int i = 0; i < c; i++) {
			heightCheck += heightDirectionCheck;
			RaycastHit[] hits = Physics.RaycastAll(heightCheck, Vector3.down, 20f);
			if (hits.Length > 0 && hits[0].point.y > highest) {
				highest = hits[0].point.y;
				a = CopyVector(hits[0].point, a);
			}
		}
		
		float y = Mathf.Max(0.5f, (highest - targetPosition.y) * 2);
		return highest + y;
	}
	
	Vector3 CopyVector(Vector3 source, Vector3 target) {
		target.x = source.x;
		target.y = source.y;
		target.z = source.z;
		
		return target;
	}
	
	Vector3 SuperSmoothLerp( Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float time, float speed )
	{
		if (speed == 0 || time == 0) return pastPosition;
		Vector3 f = pastPosition - pastTargetPosition + (targetPosition - pastTargetPosition) / (speed * time);
		return targetPosition - (targetPosition - pastTargetPosition) / (speed*time) + f * Mathf.Exp(-speed*time);
	}
	
	float SuperSmoothLerpValue (float pastValue, float pastTargetValue, float targetValue, float time, float speed)
	{
		if (speed == 0 || time == 0) return pastValue;
		float offset =  (targetValue - pastTargetValue);
		if (offset > 180) offset -= 360;
		else if (offset < -180) offset += 360;
		float f = pastValue - pastTargetValue + offset / (speed * time);
		return targetValue - offset / (speed * time) + f * Mathf.Exp(-speed * time);
	}
	
}
