using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
	
	public float moveSpeed = 0.02f;
	public ForestTerrainGenerator paths;
	public float currentRotationPause;
	public float rotationInputPausedUntil;
	public float runThreshold = 0.25f;
	public float jumpForce = 10f;
	public float gravity = 0.25f;
	
	protected Vector3 move = Vector3.zero;
	private Animator animator;
	private bool isWalking;
	private bool isRunning;
	private bool isJumping;
	private float currentJumpSpeed;
	
	[HideInInspector]
	public float yRotation;
	
	// Use this for initialization
	void Start () {
		rotationInputPausedUntil = 0;
		animator = gameObject.GetComponent<Animator>();
		yRotation = transform.localEulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () {
		bool rotateIt = false;
		if (rotationInputPausedUntil > 0) {
			currentRotationPause += Time.deltaTime;
			if (currentRotationPause > rotationInputPausedUntil) rotationInputPausedUntil = 0;
		} else {
			rotateIt = Input.GetAxis("Switch") != 0;
		}
		
		Vector3 newMove = new Vector3(Input.GetAxis("Horizontal"), move.y, 0f);
		float walk = Input.GetAxis("Walk");
		if (walk != 0) newMove.x /= 2;
		
		float speed = Mathf.Abs(newMove.x);
		isWalking = speed > 0.01f;
		isRunning = speed > runThreshold;
		
		newMove *= moveSpeed * Time.deltaTime;
		
		float vertInput = Input.GetAxis("Vertical");
		if (vertInput > 0 && newMove.y == 0) {
			currentJumpSpeed = jumpForce;
			newMove.y = currentJumpSpeed;
			isJumping = true;
		} else if (isJumping) {
			newMove.y += currentJumpSpeed;
		}
		
		if (newMove.y > 0) {
			newMove.x = move.x;
			currentJumpSpeed -= gravity;
		} else {
			newMove.y = 0;
			currentJumpSpeed = 0;
			isJumping = false;
		}
		
		move = newMove;
		UpdateForMove(move, rotateIt);
	}
	
	void UpdateForMove (Vector3 move, bool rotateIt) {
		MapLocation transitLocation = GetTransitLocation(move, yRotation);
		if (rotateIt) {
			UpdateRotation(move);
			currentRotationPause = 0;
			rotationInputPausedUntil = 0.5f;
		}
		
		
		if (transitLocation != null) {
			Vector3 transformedMove = GetTransformedMove (yRotation) * move.x;
			gameObject.transform.Translate(transformedMove, Space.World);
		} else {
			isRunning = false;
			isWalking = false;
		}
		
		Vector3 pos = gameObject.transform.localPosition;
		pos.y = paths.GetYForPosition(pos, IsRotated(yRotation));
		gameObject.transform.localPosition = pos;
		
		UpdateForestMakers();
		UpdateAnimator(move);
	}
	
	Vector3 GetTransformedMove (float yRotation) {
		if (yRotation == 180) return Vector3.left;
		else if (yRotation == 90) return Vector3.back;
		else if (yRotation == 270) return Vector3.forward;
	
		return Vector3.right;
	}
	
	void UpdateAnimator (Vector3 move) {
		animator.SetBool("IsWalking", isWalking);
		animator.SetBool("IsRunning", isRunning);
		animator.SetBool("IsJumping", isJumping);
		
		if (move.x > 0) {
			animator.SetInteger("Direction", 1);
		} else if (move.x < 0) {
			animator.SetInteger("Direction", -1);
		}
	}
	
	MapLocation GetTransitLocation (Vector3 move, float yRotation) {
		Vector3 transformedMove = GetTransformedMove (yRotation) * move.x;
		MapLocation location = paths.GetTransitLocationForMove(gameObject.transform.position, transformedMove, IsRotated(yRotation));
		
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
		bool isRotated = IsRotated(yRotation);
		float rotA = RationaliseAngle(yRotation + 90);
		float rotB = RationaliseAngle(yRotation - 90);
		
		transitLocation = GetTransitLocation(move, rotA);
		float newRot = rotA;
		if (transitLocation == null) {
			transitLocation = GetTransitLocation(move, rotB);
			newRot = rotB;
		}
		if (transitLocation == null) return;
		
		yRotation = Mathf.Round(newRot);
		Vector3 pos = gameObject.transform.localPosition;
		
		if (isRotated) {
			pos.z = Mathf.Round(pos.z);
		} else {
			pos.x = Mathf.Round(pos.x);
		}
		gameObject.transform.localPosition = pos;
	}
	
	float RationaliseAngle (float angle) {
		while (angle < 0) angle = 360 - angle;
		while (angle > 360) angle = angle - 360;
		
		return angle;
	}
	
	void UpdateForestMakers () {
		int x = Mathf.RoundToInt(gameObject.transform.localPosition.x);
		int z = Mathf.RoundToInt(gameObject.transform.localPosition.z);
		int r = Mathf.RoundToInt(yRotation);
		paths.UpdateForestAreasInFrontOfPosition(x, z, r);
	}
}