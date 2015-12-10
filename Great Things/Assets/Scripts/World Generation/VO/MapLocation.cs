using UnityEngine;
using System.Collections;

[System.Serializable]
public class MapLocation {
	public int x;
	public float y;
	public int z;
	public Vector2[] linkedLocations;
	public Color colour;
	public float[] corners;
	public string tag;
	
	public MapLocation (int x, int y, int z, string tag) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.tag = tag;
		linkedLocations = new Vector2[9];
		corners = new float[4];
	}
	
	public bool Equals (MapLocation other) {
		if (other == null) return false;
		return other.x == x && other.y == y && other.z == z;
	}
	
	public int Compare (MapLocation other) {
		//   - 0 +
		// - 0 1 2
		// 0 3 4 5
		// + 6 7 8
		
		// x - 0 + 
		// - / 0 /
		// 0 1 - 2
		// + / 3 /
		
		if (x == other.x && z == other.z) return 4;
		if (z < other.z) {
			if (x < other.x) return 0;
			else if (x > other.x) return 2;
			else return 1;
		} else if (z > other.z) {
			if (x < other.x) return 6;
			else if (x > other.x) return 8;
			else return 7;
		} else {
			if (x < other.x) return 3;
			else return 5;
		}
	}
	
	public override string ToString () {
		return "[x:" + x + " y:" + y + " z:" + z + "]";
	}
}
