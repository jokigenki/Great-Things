using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Locale : MonoBehaviour {

	// shown
	public int drawDistance = 10;
	public bool fromPrefab;
	
	// hidden
	public List<GameObject> sublocales;
	public SeededRandomiser randomiser;
	public Map map;
	
	// private
	Vector3 lastUpdatePosition;
	
	// Use this for initialization
	void Start () {
	
		if (fromPrefab) {
			FromPrefab();
		}
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void FromPrefab() {
		GameObject player = GameObject.Find("Player");
		if (player != null) {
			PositionPlayer(player);
			SetReady(player);
		}
	}
	
	public void SetRandomiser (SeededRandomiser randomiser) {
		this.randomiser = randomiser;
	}
	
	// Create the player avatar at the map entrance
	// TODO: expand this so we can move the player to various entrances
	public void PositionPlayer (GameObject player)
	{
		MapLocation entrance = (MapLocation)map.entrances [0];
		player.SetActive(true);
		Vector3 playerPosition = new Vector3 (entrance.x, entrance.y + 0.5f + (player.transform.localScale.y / 2), entrance.z);
		player.transform.position = playerPosition;
		Renderer renderer = player.GetComponent<Renderer>();
		renderer.enabled = false;
	}
	
	public void SetReady (GameObject player) {
		Renderer renderer = player.GetComponent<Renderer>();
		renderer.enabled = true;
		
		PlayerControl control = player.GetComponent<PlayerControl>();
		control.locale = this;
	}
	
	// turn the sublocale renderers off if they are outside the render area
	public void UpdateAreaVisibility (int x, int z, int rotation) {
		if (UpdateHasNotChanged(x, z, rotation)) return;
		Rect distRect = new Rect(x - drawDistance, z - drawDistance, drawDistance * 2, drawDistance * 2); 
		foreach (GameObject area in sublocales) {
			Sublocale sublocale = area.GetComponent<Sublocale> ();
			sublocale.RendererEnabled =  sublocale.Overlaps(distRect);
		}
	}
	
	// If the given value is the same as the value we last called this method with, return true
	bool UpdateHasNotChanged (int x, int z, int rotation) {
		if (lastUpdatePosition.x == x &&
		    lastUpdatePosition.y == z &&
		    lastUpdatePosition.z == rotation) return true;
		
		lastUpdatePosition.x = x;
		lastUpdatePosition.y = z;
		lastUpdatePosition.z = rotation;
		
		return false;
	}
	
	// Returns the map location that is at current position plus the move at the current rotation
	public MapLocation GetTransitLocationForMove (Vector3 position, Vector3 move, bool rotated) {
		if (map == null) return null; 
		return rotated ? map.GetZLocation (position, move) : map.GetXLocation (position, move);
	}
	
	// Given a position, returns the y position at that map location
	public float GetYForPosition (Vector3 position) {
		if (map == null) return 0f;
		return map.GetPositionHeight(position.x, position.z);
	}
}
