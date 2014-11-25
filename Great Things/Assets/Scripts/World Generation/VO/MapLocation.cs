using UnityEngine;
using System.Collections;

public class MapLocation {
	public int x;
	public int y;
	public int z;
	public bool isEntrance;
	public bool isExit;
	public MapLocation[] linkedLocations;
	public Color colour;
	
	public MapLocation (int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
		linkedLocations = new MapLocation[9];
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
