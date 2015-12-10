using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapUtils {

	// Is the given position within the map?
	public static bool IsMapPositionValid (int x, int z, int mapWidth, int mapDepth) {
		if (x < 0 || x >= mapWidth || z < 0 || z >= mapDepth) return false;
		
		return true;
	}
	
	// Iterates through each location in the map and gives it a randomised height
	// then builds an array containing the heights of each location quad's corners
	public static void SetHeightsForMap (Map map, SeededRandomiser randomiser)
	{
		int hilliness = map.hilliness;
		int mapWidth = map.mapWidth;
		int mapDepth = map.mapDepth;
		if (hilliness == 0)
			return;
		int c = hilliness * 100;
		for (int i = 0; i < c; i++) {
			int x = Mathf.RoundToInt (randomiser.GetRandomFromRange (0, mapWidth));
			int z = Mathf.RoundToInt (randomiser.GetRandomFromRange (0, mapDepth));
			
			if (IsNextToWater(map, x, z)) {
				float value = (randomiser.GetRandom () * 0.1f) - 0.05f;
				ChangeHeightForPosition (x, z, map, value, true);
				c--;
			} else {
				float value = (randomiser.GetRandom () * 0.5f) - 0.25f;
				ChangeHeightForPosition (x, z, map, value, true);
			}
		}
		
		SetCornerHeights(map);
	}
	
	public static bool IsNextToWater (Map map, int x, int z) {
		return IsWater(map, x+1, z) ||
			IsWater(map, x, z+1) ||
				IsWater(map, x-1, z) ||
				IsWater(map, x, z-1) ||
				IsWater(map, x+1, z+1) ||
				IsWater(map, x-1, z+1) ||
				IsWater(map, x+1, z-1) ||
				IsWater(map, x-1, z-1);
	}
	
	public static bool IsWater (Map map, int x, int z) {
		if (x < 0 || x >= map.mapWidth) return false;
		if (z < 0 || z >= map.mapDepth) return false;
		
		bool nextToPool = map.GetMapLocationForPosition(x, z).tag.Equals("pool");
		return nextToPool;
	}
	
	// changes the height for the location at xz
	// if recurse is true, then the heights of surrounding tiles will be modified to give a smoother contour
	public static void ChangeHeightForPosition (int x, int z, Map map, float value, bool recurse)
	{
		if (!MapUtils.IsMapPositionValid(x, z, map.mapWidth, map.mapDepth)) return;
		MapLocation location = map.GetMapLocationForPosition(x, z);
		if (location == null) return;
		float currentValue = location.y;
		currentValue += value;
		location.y = currentValue;
		
		if (recurse) {
			ChangeHeightForSurroundingLocations(x, z, map, value);
		}
	}
	
	// Changes the heights of the tiles around the given tile
	public static void ChangeHeightForSurroundingLocations (int x, int z, Map map, float value) {
		float halfValue = value * 0.5f;
		float quarterValue = value * 0.25f;
		
		ChangeHeightForPosition (x - 1, z, map, halfValue, false);
		ChangeHeightForPosition (x - 2, z, map, quarterValue, false);
		ChangeHeightForPosition (x + 1, z, map, halfValue, false);
		ChangeHeightForPosition (x + 2, z, map, quarterValue, false);
		ChangeHeightForPosition (x, z - 1, map, halfValue, false);
		ChangeHeightForPosition (x, z - 2, map, quarterValue, false);
		ChangeHeightForPosition (x, z + 1, map, halfValue, false);
		ChangeHeightForPosition (x, z + 2, map, quarterValue, false);
		ChangeHeightForPosition (x - 1, z - 1, map, quarterValue, false);
		ChangeHeightForPosition (x - 1, z + 1, map, quarterValue, false);
		ChangeHeightForPosition (x + 1, z - 1, map, quarterValue, false);
		ChangeHeightForPosition (x + 1, z + 1, map, quarterValue, false);
	}
	
	// Sets the corner heights each of the map locations, which can then be used to set the vertex height positions
	public static void SetCornerHeights (Map map) {
		
		for (int z = 0; z < map.mapDepth; z++) {
			for (int x = 0; x < map.mapWidth; x++) {
				MapLocation location = map.GetMapLocationForPosition(x, z);
				for (int i = 0; i < 4; i++) {
					location.corners[i] = MapUtils.GetHeightForPosition(map, x, z, i);
				}
			}
		}
	}
	
	// Finds the average height for the 4 location corners surrounding the given vertex at the xz position
	public static float GetHeightForPosition (Map map, int x, int z, int vertex) {
		
		MapLocation location = map.GetMapLocationForPosition(x, z);
		float a = 0;
		float b = 0;
		float c = 0;
		float d = location == null ? 0 : location.y;
		
		if (vertex == 0 || vertex == 1) {
			c = GetHeightForPosition(map, x, z + 1);
		}
		
		if (vertex == 3 || vertex == 2) {
			c = GetHeightForPosition(map, x, z - 1);
		}
		
		if (vertex == 0 || vertex == 2) {
			b = GetHeightForPosition(map, x + 1, z);
		}
		
		if (vertex == 1 || vertex == 3) {
			b = GetHeightForPosition(map, x - 1, z);
		}
		
		if (vertex == 0) {
			a = GetHeightForPosition(map, x + 1, z + 1);	
		} else if (vertex == 1) {
			a = GetHeightForPosition(map, x - 1, z + 1);
		} else if (vertex == 2) {
			a = GetHeightForPosition(map, x + 1, z - 1);
		} else if (vertex == 3) {
			a = GetHeightForPosition(map, x - 1, z - 1);
			
		}
		
		return (a + b + c + d) / 4f;
	}
	
	// Gets the height for the position, or 0 if the position is not valid
	public static float GetHeightForPosition (Map map, int x, int z) {
		if (IsMapPositionValid(x, z, map.mapWidth, map.mapDepth)) return map.GetMapLocationForPosition(x, z).y;
		return 0;
	}
}
