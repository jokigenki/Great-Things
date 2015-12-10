using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
	
	public float moveSpeed = 0.02f;
	public Forest forest;
	public float runThreshold = 0.25f;
	public float jumpForce = 10f;
	public float gravity = 0.25f;
	
	protected Vector3 move = Vector3.zero;
	private Animator animator;
	private bool isWalking;
	private bool isRunning;
	private bool isJumping;
	private float currentJumpSpeed;
	private Vector3 lastMove;
	
	private bool rotateReleased;
	
	[HideInInspector]
	public float yRotation;
	
	// Use this for initialization
	void Start () {
		animator = gameObject.GetComponent<Animator>();
		yRotation = transform.localEulerAngles.y;
		rotateReleased = true;
	}
	
	// Update is called once per frame
	void Update () {
	
		if (forest == null) return;
		
		bool rotateIt = false;
		rotateIt = Input.GetAxis("Switch") != 0;
		
		if (!rotateReleased) {
			if (!rotateIt) rotateReleased = true;
			else rotateIt = false;
		}
		if (rotateIt) rotateReleased = false; 
					
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
		
		if (move.x != 0) lastMove = move;
	}
	
	void UpdateForMove (Vector3 move, bool rotateIt) {
		MapLocation transitLocation = GetTransitLocation(move, yRotation);
		if (rotateIt) {
			UpdateRotation(move);
		}
		
		
		if (transitLocation != null) {
			Vector3 transformedMove = GetTransformedMove (yRotation) * move.x;
			gameObject.transform.Translate(transformedMove, Space.World);
		} else {
			isRunning = false;
			isWalking = false;
		}
		
		if (forest != null) {
			Vector3 pos = gameObject.transform.localPosition;
			pos.y = forest.GetYForPosition(pos);
			gameObject.transform.localPosition = pos;
		}
		
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
		if (forest == null) return null;
		Vector3 transformedMove = GetTransformedMove (yRotation) * move.x;
		MapLocation location = forest.GetTransitLocationForMove(gameObject.transform.position, transformedMove, IsRotated(yRotation));
		
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
		
		if (move.x == 0) move = lastMove;
		transitLocation = GetTransitLocation(move, rotA);
		float newRot = rotA;
		if (transitLocation == null) {
			transitLocation = GetTransitLocation(move, rotB);
			newRot = rotB;
		}
		
		if (transitLocation == null) {
			if (move.x > 0) newRot = rotA;
			else newRot = rotB;
		}
		
		yRotation = Mathf.Round(newRot);
		Vector3 pos = gameObject.transform.localPosition;
		
		if (isRotated) {
			pos.z = Mathf.Round(pos.z);
		} else {
			pos.x = Mathf.Round(pos.x);
		}
		gameObject.transform.localPosition = pos;
	}
	
	// Return a value between 
	float RationaliseAngle (float angle) {
		while (angle < 0) angle = 360 - angle;
		while (angle > 360) angle = angle - 360;
		
		return angle;
	}
	
	void UpdateForestMakers () {
		int x = Mathf.RoundToInt(gameObject.transform.localPosition.x);
		int z = Mathf.RoundToInt(gameObject.transform.localPosition.z);
		int r = Mathf.RoundToInt(yRotation);
		if (forest != null) forest.UpdateForestAreaVisibility (x, z, r);
	}
}