using UnityEngine;
using System.Collections;

public class Map {

	public Texture2D texture;
	public int hilliness;
	public int mapWidth;
	public int mapDepth;
	public ArrayList entrances;
	public ArrayList exits;
	public MapLocation[,] locations;
	public Color[] pixels;
	public MapTag blankTag;
	public MapTag[] tags;
	
	public Map (Texture2D texture, MapTag blankTag, MapTag[] tags, int hilliness) {
		this.texture = texture;
		this.hilliness = hilliness;
		this.blankTag = blankTag;
		this.tags = tags;
		mapWidth = texture.width;
		mapDepth = texture.height;
		entrances = new ArrayList ();
		exits = new ArrayList();
		locations = new MapLocation[mapWidth, mapDepth];
		pixels = texture.GetPixels (0, 0, mapWidth, mapDepth);
		
		for (int z = 0; z < mapDepth; z++) {
			for (int x = 0; x < mapWidth; x++) {
				CreateMapLocation(x, z);
			}
		}
	}
	
	public string GetTagFromPixel (Color color, MapTag[] tags) {
		foreach (MapTag tag in tags) {
			if (tag.pixel.Equals(color)) return tag.tag;
		}
		
		return "empty";
	}
	
	public Color GetPixelForLocation (int x, int z)
	{
		if (!MapUtils.IsMapPositionValid(x, z, mapWidth, mapDepth)) return blankTag.pixel;
		return pixels [(z * mapWidth) + x];
	}
	
	public string GetTagForLocation (int x, int z) {
		Color pixel = GetPixelForLocation (x, z);
		if (pixel.Equals (blankTag.pixel)) return blankTag.tag;
		foreach (MapTag tag in tags) {
			if (pixel.Equals (tag.pixel)) return tag.tag;
		}
		
		return blankTag.tag;
	}
	
	public MapLocation GetMapLocationForPosition (Vector3 position)
	{
		return GetMapLocationForPosition (Mathf.RoundToInt (position.x), Mathf.RoundToInt (position.z));
	}
	
	public MapLocation GetMapLocationForPosition (int x, int z)
	{
		if (!MapUtils.IsMapPositionValid(x, z, mapWidth, mapDepth)) return null;
		MapLocation location = locations [x, z];
		return location;
	}
	
	public MapLocation GetXLocation (Vector3 position, Vector3 move)
	{
		int newX = Mathf.RoundToInt (position.x);
		int newZ = Mathf.RoundToInt (position.z);
		
		MapLocation xLocation = null;
		if (move.x > 0) {
			newX = Mathf.CeilToInt (position.x + move.x);
			xLocation = GetMapLocationForPosition (newX, newZ);
		} else if (move.x < 0) {
			newX = Mathf.FloorToInt (position.x + move.x);
			xLocation = GetMapLocationForPosition (newX, newZ);
		}
		
		return xLocation;
	}
	
	public MapLocation GetZLocation (Vector3 position, Vector3 move)
	{
		int newX = Mathf.RoundToInt (position.x);
		int newZ = Mathf.RoundToInt (position.z);
		
		MapLocation zLocation = null;
		if (move.z > 0) {
			newZ = Mathf.CeilToInt (position.z + move.z);
			zLocation = GetMapLocationForPosition (newX, newZ);
		} else if (move.z < 0) {
			newZ = Mathf.FloorToInt (position.z + move.z);
			zLocation = GetMapLocationForPosition (newX, newZ);
		}
		
		return zLocation;
	}
	
	public MapLocation CreateLocation (int x, int z) {
		Color color = GetPixelForLocation(x, z);
		string tag = GetTagFromPixel(color, tags);
		if (!MapUtils.IsMapPositionValid(x, z, mapWidth, mapDepth)) return null;
		
		MapLocation location = new MapLocation (x, 0, z, tag);
		locations[x, z] = location;
		if (tag.Equals("entrance"))
			entrances.Add (location);
		location.colour = color;	
		return location;
	}
	
	public void CreateMapLocation (int x, int z)
	{
		MapLocation centre = CreateLocation (x, z);
		centre.exits [0] = CreateLocation (x, z + 1); // north
		centre.exits [1] = CreateLocation (x + 1, z); // east
		centre.exits [2] = CreateLocation (x, z - 1); // south
		centre.exits [3] = CreateLocation (x - 1, z); // west
		
		locations [x, z] = centre;
	}
	
	public float GetPositionHeight (float x, float z, bool rotated) {
		MapLocation location = GetMapLocationForPosition (Mathf.RoundToInt (x), Mathf.RoundToInt (z));
		
		RaycastHit[] hits = Physics.RaycastAll(new Vector3(x, location.y + 1f, z), new Vector3(0, -1f, 0), 2f);
		if (hits.Length == 0) return location.y;
		
		return hits[0].point.y;
	}
	
	void DrawIt (float x, float y, float z, Color c) {
		Debug.DrawLine(new Vector3(x, y + 0.5f, z), new Vector3(x, y + 1.5f, z), c, 1);
	}
}
