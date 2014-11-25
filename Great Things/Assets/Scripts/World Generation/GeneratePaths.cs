using UnityEngine;
using System.Collections;

public class GeneratePaths : MonoBehaviour
{
	public Texture2D pathMap;
	public GameObject playerPrefab;
	public Camera playerCamera;
	public Material mat;
	public ArrayList entrances;
	public static readonly Color BLANK_PIXEL = new Color (0, 0, 0f, 1f);
	public static readonly Color ENTRANCE_PIXEL = new Color (1f, 1f, 1f, 1f);
	
	//
	MapLocation[,] locations;
	Color[] pathValues;
	int mapWidth;
	int mapDepth;
	
	// Use this for initialization
	void Start ()
	{
		
		entrances = new ArrayList ();
		mapWidth = pathMap.width;
		mapDepth = pathMap.height;
		pathValues = pathMap.GetPixels (0, 0, mapWidth, mapDepth);
		locations = new MapLocation[mapWidth, mapDepth];
	
		for (int z = 0; z < mapDepth; z++) {
			for (int x = 0; x < mapWidth; x++) {
				CreateMapLocation (x, z);
			}
		}
		
		Vector3[] points = new Vector3[] {
			new Vector3 (0, -2.5f, 0),
			new Vector3 (0, -2.5f, 0),
			new Vector3 (0, 0.5f, 0),
			new Vector3 (0, 0.5f, 0)};
		Vector3 position = Vector3.zero;
		for (int z = 0; z < mapDepth; z++) {
			for (int x = 0; x < mapWidth; x++) {
			
				position.x = x;
				position.y = 0; // TODO: y values
				position.z = z;
				
				MapLocation location = GetMapLocationForPosition (x, z, false);
				if (location == null)
					continue;
				
				foreach (MapLocation linkedLocation in location.linkedLocations) {
					if (linkedLocation == null)
						continue;
					bool reverse = SetPointValues (points, x, z, linkedLocation);
					
					GameObject tile = MeshUtils.CreatePlane (points, mat, reverse);
					//tile.renderer.material.color = GetPixelForLocation (x, z);
					tile.transform.position = position;
					tile.transform.parent = gameObject.transform;
				}
			}
		}
		
		MapLocation entrance = (MapLocation)entrances [0];
		GameObject player = (GameObject)Instantiate (playerPrefab, Vector3.zero, Quaternion.identity);
		Vector3 playerPosition = new Vector3 (entrance.x, entrance.y + 0.5f + (player.transform.localScale.y / 2), entrance.z);
		player.transform.position = playerPosition;
		PlayerControl control = player.GetComponent<PlayerControl> ();
		control.paths = this;
		
		SmoothCameraTracker tracker = playerCamera.GetComponent<SmoothCameraTracker>();
		tracker.target = player;
		
		//AlignCameraToPlayer aligner = playerCamera.GetComponent<AlignCameraToPlayer>();
		//aligner.player = player;
	}
	
	bool SetPointValues (Vector3[] points, int x, int z, MapLocation otherLocation)
	{
		points [0].x = 0;
		points [1].x = 0;
		points [2].x = 0;
		points [3].x = 0;
		// 0:bottom-left, 1:bottom-right, 2:top-left, 3:top-right
		if (otherLocation.x < x) {
			points [0].x = -0.5f;
			points [2].x = -0.5f;
		} else if (otherLocation.x > x) {
			points [1].x = 0.5f;
			points [3].x = 0.5f;
		}
		
		points [0].z = 0;
		points [1].z = 0;
		points [2].z = 0;
		points [3].z = 0;
		// 0:bottom-left, 1:bottom-right, 2:top-left, 3:top-right
		if (otherLocation.z < z) {
			if (otherLocation.x < x) {
				points [0].z = -0.5f;
				points [2].z = -0.5f;
			} else {
				points [1].z = -0.5f;
				points [3].z = -0.5f; 
			}
			
		} else if (otherLocation.z > z) {
			if (otherLocation.x < x) {
				points [0].z = 0.5f;
				points [2].z = 0.5f;
			} else {
				points [1].z = 0.5f;
				points [3].z = 0.5f;
			}
			if (otherLocation.x == x)
				return true;
		}
		
		return false;
	}
	
	void ReversePoints (Vector3[] points)
	{
		
		Vector3 p0 = points [0];
		Vector3 p1 = points [2];
		Vector3 p2 = points [1];
		Vector3 p3 = points [3];
		
		points [0] = p3;
		points [1] = p2;
		points [2] = p1;
		points [3] = p0;
	}
	
	void CreateMapLocation (int x, int z)
	{
	
		Color pixel = GetPixelForLocation (x, z);
		if (pixel.Equals (BLANK_PIXEL))
			return;
		
		MapLocation[,] nineLocations = new MapLocation[3, 3];
		for (int zc = -1; zc < 2; zc++) {
			int zp = z + zc;
			for (int xc = -1; xc < 2; xc++) {
				int xp = x + xc;
				pixel = GetPixelForLocation (xp, zp);
				if (pixel.Equals (BLANK_PIXEL))
					continue;
				
				MapLocation location = GetMapLocationForPosition (xp, zp, true);
				// TODO: add y value from map colour
				
				if (xp == 0 || xp == mapWidth - 1 || zp == 0 || zp == mapDepth - 1)
					location.isExit = true;
				if (pixel.Equals (ENTRANCE_PIXEL) || pixel == ENTRANCE_PIXEL) {
					location.isEntrance = true;
					entrances.Add (location);
				}
				location.colour = pixel;
				nineLocations [xc + 1, zc + 1] = location;
			}
		}
		
		MapLocation centreLocation = nineLocations [1, 1];
		for (int zc = 0; zc < 3; zc++) {
			for (int xc = 0; xc < 3; xc++) {
				if (xc == 1 && zc == 1)
					continue;
				MapLocation location = nineLocations [xc, zc];
				centreLocation.linkedLocations [(zc * 3) + xc] = location;
			}
		}
		
		locations [x, z] = centreLocation;
	}
	
	MapLocation last;
	public MapLocation GetExitForMove (Vector3 position, Vector3 move)
	{	
		int cx = Mathf.RoundToInt (position.x);
		int cz = Mathf.RoundToInt (position.z); 
		int x = Mathf.RoundToInt (cx + move.x);
		int z = Mathf.RoundToInt (cz + move.z);
		float xRem = position.x - x;
		float zRem = position.z - z;
		if (xRem == 0 && zRem == 0)
			return GetMapLocationForPosition (x, z);
		
		int exitX = 0;
		int exitZ = 0;
		if (xRem > 0)
			exitX = 1;
		else if (xRem < 0)
			exitX = -1;
		if (zRem > 0)
			exitZ = 1;
		else if (zRem < 0)
			exitZ = -1;
		
		MapLocation currentExit = GetMapLocationForPosition (x + exitX, z + exitZ);
		if (currentExit != null) {
			if (!currentExit.Equals(last)) {
				last = currentExit;
				//print ("pos:" + position.x + "," + position.z + " c:"+ cx + "," + cz + " xz:" + x + "," + z + " ex:" + exitX + "," + exitZ + " rem:" + xRem + "," + zRem + " m:" + move.x + "," + move.z);
			}
			return currentExit;
		}
		return ChooseExitForDirection (GetMapLocationForPosition (cx, cz), GetPreferredExitsForDirection (exitX, exitZ));
	}
	
	int[] GetPreferredExitsForDirection (int x, int z)
	{
		//   - 0 +
		// + 6 7 8
		// 0 3 4 5
		// - 0 1 2
		//print ("x" + x + "z" + z);
		if (z == 0) {
			if (x == 1) return new int[]{5, 2, 8};
			else if (x == -1) return new int[]{3, 6, 0};
			else return new int[]{};
		} else if (z == -1) {
			if (x == 1) return new int[]{2, 5, 1};
			else if (x == -1) return new int[]{0, 3, 1};
			else return new int[]{1, 2, 0};
		} else {
			if (x == 1) return new int[]{2, 5, 1};
			else if (x == -1) return new int[]{0, 3, 1};
			else return new int[]{1, 2, 0};
		}
	}
	
	MapLocation ChooseExitForDirection (MapLocation currentLocation, int[] preferredExits)
	{		
		if (currentLocation == null) return null;
		
		for (int i = 0; i < preferredExits.Length; i++) {
			MapLocation location = currentLocation.linkedLocations [preferredExits [i]];
			if (location != null) {
				//print ("location for:" + preferredExits [i] + " = " + location + "c:" + currentLocation.x + "," + currentLocation.z);
				return location;
			}
		}
		return null;
	}
	
	public Vector3 GetRotationForExit (MapLocation currentLocation, MapLocation exitLocation)
	{
		//print ("cx:" + currentLocation.x + " z:" + currentLocation.z + " ex:" + exitLocation.x + " z:" + exitLocation.z);
		if (currentLocation.z == exitLocation.z)
			return Vector3.zero;
		if (currentLocation.x == exitLocation.x)
			return new Vector3 (0, 90, 0);
		
		if (exitLocation.x > currentLocation.x) {
			if (exitLocation.z > currentLocation.z)
				return new Vector3 (0, 315, 0);
			else
				return new Vector3 (0, 45, 0);
		} else {
			if (exitLocation.z > currentLocation.z)
				return new Vector3 (0, 45, 0);
			else
				return new Vector3 (0, 315, 0);
		}
	}
	
	public MapLocation GetMapLocationForPosition (Vector3 position)
	{
		return GetMapLocationForPosition (Mathf.RoundToInt (position.x), Mathf.RoundToInt (position.z), false);
	}
	
	MapLocation GetMapLocationForPosition (int x, int z)
	{
		return GetMapLocationForPosition (x, z, false);
	}
	
	MapLocation GetMapLocationForPosition (int x, int z, bool createNewIfMissing)
	{
		MapLocation location = locations [x, z];
		if (location != null)
			return location;
		if (createNewIfMissing)
			return new MapLocation (x, 0, z);
		
		return null;
	}
	
	Color[,] GetPixelsForLocation (int x, int z)
	{
		
		Color[,] pixels = new Color[3, 3];
		int zp = 0;
		for (int zc = z - 1; zc < z + 2; zc++) {
			int xp = 0;
			for (int xc = x - 1; xc < x + 2; xc++) {
				pixels [xp, zp] = GetPixelForLocation (xc, zc);
			}
		}
		
		return pixels;
	}
	
	Color GetPixelForLocation (int x, int z)
	{
		if (x < 0 || x >= mapWidth || z < 0 || z >= mapDepth)
			return BLANK_PIXEL;
		return pathValues [(z * mapWidth) + x];
	}
}
