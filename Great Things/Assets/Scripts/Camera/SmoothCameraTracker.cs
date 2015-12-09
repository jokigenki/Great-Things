using UnityEngine;
using System.Collections;

public class SmoothCameraTracker : MonoBehaviour {

	public GameObject target;
	public GameObject followCamera;
	public float easeSpeed = 0.005f;
	public float yOffset = 0.5f;
	Vector3 targetPosition;
	
	float targetRotation;
	Vector3 currentRotation;
	float distance = 5;
	
	//Vector3 followerVelocity;
	Vector3 pastCameraPosition, pastTargetPosition;
	//float pastCameraRotation, pastTargetRotation;
	
	PlayerControl control;
	
	bool firstPositionSet;
	
	void Start()
	{
		targetRotation = 0;
		control = target.GetComponent<PlayerControl>();
	}
	
	void Update()
	{	
		Transform targetTransform = target.transform;
		Transform camTransform = followCamera.transform;
		
		float playerRotation = control != null ? control.yRotation : targetTransform.eulerAngles.y;
		float x = distance * Mathf.Sin(playerRotation * Mathf.PI / 180);
		float z = distance * Mathf.Cos(playerRotation * Mathf.PI / 180);
		
		targetPosition.x = targetTransform.position.x - x;
		targetPosition.y = targetTransform.position.y + yOffset;
		targetPosition.z = targetTransform.position.z - z;
		targetRotation = playerRotation;
		
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
		float lookAtRotation = Mathf.Atan2(targetTransform.position.x - camTransform.position.x, targetTransform.position.z - camTransform.position.z) / Mathf.PI * 180;
		currentRotation.y = lookAtRotation;
		//pastCameraRotation = currentRotation.y;
		//pastTargetRotation = targetRotation;
		followCamera.transform.localEulerAngles = currentRotation;
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
