using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public float moveSpeed = 0.02f;
	protected Vector3 move = Vector3.zero;
	public ForestTerrainGenerator paths;
 	public float currentPause;
 	public float inputPausedUntil;
 	
	// Use this for initialization
	void Start () {
		inputPausedUntil = 0;
	}
	
	// Update is called once per frame
	void Update () {
		
		bool rotateIt = false;
		if (inputPausedUntil > 0) {
			currentPause += Time.deltaTime;
			if (currentPause > inputPausedUntil) inputPausedUntil = 0;
		} else {
			rotateIt = Input.GetAxis("Jump") != 0;
		}
		
		move = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);
		move.Normalize();
		move *= moveSpeed * Time.deltaTime;
		
		UpdateForMove(move, rotateIt);
	}

	void UpdateForMove (Vector3 move, bool rotateIt) {
		MapLocation transitLocation = GetTransitLocation(move, false);
		if (rotateIt) {
			UpdateRotation(move);
			currentPause = 0;
			inputPausedUntil = 0.5f;
		}
		if (transitLocation != null) {
			Vector3 transformedMove = transform.right * move.x;
			transform.Translate(transformedMove, Space.World);
		}
		
		Vector3 pos = transform.localPosition;
		pos.y = paths.GetYForPosition(pos, IsRotated(transform.localEulerAngles.y)) + (transform.localScale.y / 2);
		transform.localPosition = pos;
		
		UpdateForestMakers();
	}
	
	MapLocation GetTransitLocation (Vector3 move, bool isPerpendicular) {
		Vector3 transformedMove = transform.TransformDirection(move);
		MapLocation location = isPerpendicular
			? paths.GetPerpendicularTransitLocationForMove(transform.position, transformedMove, IsRotated(transform.localEulerAngles.y))
			: paths.GetTransitLocationForMove(transform.position, transformedMove, IsRotated(transform.localEulerAngles.y));
		
		if (location == null || (!location.tag.Equals("path") && !location.tag.Equals("entrance"))) return null;
		return location;
	}
	
	bool IsRotated (float current) {
		return MatchesRotation(current, 90) ||
				MatchesRotation(current, 270);
	}
	
	bool MatchesRotation(float current, float target) {
		return Mathf.Abs(current - target) < 0.01f;
	}
	
	void UpdateRotation (Vector3 move) {
	
		MapLocation transitLocation = null;
		Vector3 rotation = transform.localEulerAngles;
		bool isRotated = IsRotated(rotation.y);
		float a = move.x < 0 ? 1f : -1f;
		float b = move.x < 0 ? -1f : 1f;
		float rotA = rotation.y + 90;
		float rotB = rotation.y - 90;
		
		transitLocation = GetTransitLocation(new Vector3(0f, 0f, a), true);
		rotation.y = rotA;
		if (transitLocation == null) {
			transitLocation = GetTransitLocation(new Vector3(0f, 0f, b), true);
			rotation.y = rotB;
		}
		if (transitLocation == null) return;
		
		rotation.y = Mathf.Round(rotation.y);
		Vector3 pos = transform.localPosition;
		
		if (isRotated) {
			pos.z = Mathf.Round(pos.z);
		} else {
			pos.x = Mathf.Round(pos.x);
		}
		transform.localEulerAngles = rotation;
		transform.localPosition = pos;	 
	}
	
	void UpdateForestMakers () {
		int x = Mathf.RoundToInt(transform.localPosition.x);
		int z = Mathf.RoundToInt(transform.localPosition.z);
		int r = Mathf.RoundToInt(transform.localEulerAngles.y);
		paths.UpdateForestAreasInFrontOfPosition(x, z, r);
	}
}
