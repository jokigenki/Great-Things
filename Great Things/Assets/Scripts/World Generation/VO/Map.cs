using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Map {

	public Texture2D texture;
	public int hilliness;
	public int mapWidth;
	public int mapDepth;
	public List<MapLocation> entrances;
	public List<MapLocation> locations;
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
		entrances = new List<MapLocation>();
		locations = new List<MapLocation>();
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
		 if (x < 0 || z < 0 || x > mapWidth || z > mapDepth) return null;
		int index = z * mapWidth + x;
		return locations[index];
	}
	
	public MapLocation GetXLocation (Vector3 position, Vector3 move) {
		int newX = move.x > 0 ? Mathf.CeilToInt (position.x + move.x) : Mathf.FloorToInt (position.x + move.x);
		int newZ = Mathf.RoundToInt (position.z);
		MapLocation xLocation = GetMapLocationForPosition (newX, newZ);
		
		return xLocation;
	}
	
	public MapLocation GetZLocation (Vector3 position, Vector3 move) {
		int newX = Mathf.RoundToInt (position.x);
		int newZ = move.z > 0 ? Mathf.CeilToInt (position.z + move.z) : Mathf.FloorToInt (position.z + move.z);
		MapLocation zLocation = GetMapLocationForPosition (newX, newZ);
		
		return zLocation;
	}
	
	public MapLocation CreateLocation (int x, int z) {
		Color color = GetPixelForLocation(x, z);
		string tag = GetTagFromPixel(color, tags);
		if (!MapUtils.IsMapPositionValid(x, z, mapWidth, mapDepth)) return null;
		
		MapLocation location = new MapLocation (x, 0, z, tag);
		PutLocation(x, z, location);
		if (tag.Equals("entrance"))
			entrances.Add (location);
		location.colour = color;	
		return location;
	}
	
	void PutLocation (int x, int z, MapLocation item) {
		int index = z * mapWidth + x;
		locations.Insert(index, item);
	}
	
	public void CreateMapLocation (int x, int z)
	{
		MapLocation centre = CreateLocation (x, z);
		PutLocation(x, z, centre);
	}
	
	public float GetPositionHeight (float x, float z) {
		
		Vector3 start = new Vector3(x, 10f, z);
		RaycastHit[] hits = Physics.RaycastAll(start, Vector3.down, 20f);
		if (hits.Length == 0) return GetMapLocationForPosition (Mathf.RoundToInt (x), Mathf.RoundToInt (z)).y;
		
		//Debug.DrawLine(start, hits[0].point, Color.cyan, 2f);
		return hits[0].point.y;
	}
	
	void DrawIt (float x, float y, float z, Color c) {
		Debug.DrawLine(new Vector3(x, y + 0.5f, z), new Vector3(x, y + 1.5f, z), c, 1);
	}
}
