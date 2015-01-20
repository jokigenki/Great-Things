using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	public GameObject target;
	public float moveSpeed = 0.02f;
	protected Vector3 move = Vector3.zero;
	public ForestTerrainGenerator paths;
 	public float currentRotationPause;
 	public float rotationInputPausedUntil;
 	
 	private Animator animator;
 	
	// Use this for initialization
	void Start () {
		rotationInputPausedUntil = 0;
		animator = target.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		
		bool rotateIt = false;
		if (rotationInputPausedUntil > 0) {
			currentRotationPause += Time.deltaTime;
			if (currentRotationPause > rotationInputPausedUntil) rotationInputPausedUntil = 0;
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
			currentRotationPause = 0;
			rotationInputPausedUntil = 0.5f;
		}
		if (transitLocation != null) {
			Vector3 transformedMove = target.transform.right * move.x;
			target.transform.Translate(transformedMove, Space.World);
		}
		
		Vector3 pos = target.transform.localPosition;
		pos.y = paths.GetYForPosition(pos, IsRotated(target.transform.localEulerAngles.y)) + (target.transform.localScale.y / 2);
		target.transform.localPosition = pos;
		
		UpdateForestMakers();
		
		UpdateAnimator(move);
	}
	
	void UpdateAnimator (Vector3 move) {
		if (move.x > 0) {
			animator.SetInteger("Direction", 1);
			animator.SetBool("IsMoving", true);
		} else if (move.x < 0) {
			animator.SetInteger("Direction", -1);
			animator.SetBool("IsMoving", true);
		} else {
			animator.SetBool("IsMoving", false);
		}
	}
	
	MapLocation GetTransitLocation (Vector3 move, bool isPerpendicular) {
		Vector3 transformedMove = target.transform.TransformDirection(move);
		MapLocation location = isPerpendicular
			? paths.GetPerpendicularTransitLocationForMove(target.transform.position, transformedMove, IsRotated(target.transform.localEulerAngles.y))
				: paths.GetTransitLocationForMove(target.transform.position, transformedMove, IsRotated(target.transform.localEulerAngles.y));
		
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
		Vector3 rotation = target.transform.localEulerAngles;
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
		Vector3 pos = target.transform.localPosition;
		
		if (isRotated) {
			pos.z = Mathf.Round(pos.z);
		} else {
			pos.x = Mathf.Round(pos.x);
		}
		target.transform.localEulerAngles = rotation;
		target.transform.localPosition = pos;	 
	}
	
	void UpdateForestMakers () {
		int x = Mathf.RoundToInt(target.transform.localPosition.x);
		int z = Mathf.RoundToInt(target.transform.localPosition.z);
		int r = Mathf.RoundToInt(target.transform.localEulerAngles.y);
		paths.UpdateForestAreasInFrontOfPosition(x, z, r);
	}
}
