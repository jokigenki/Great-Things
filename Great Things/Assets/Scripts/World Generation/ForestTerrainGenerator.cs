using UnityEngine;
using System.Collections;

public class ForestTerrainGenerator : MonoBehaviour
{
	public Material treeMaterial;
	public Material bushMaterial;
	public Material groundMaterial;
	public Texture2D pathMap;
	public GameObject playerPrefab;
	public Camera playerCamera;
	public Material pathMaterial;
	public GameObject floorPrefab;
	public int hilliness = 2;
	
	GameObject forest;
	GameObject paths;
	
	ArrayList forestAreas;
	SeededRandomiser randomiser;
	Map map;
	
	public static readonly Color BLANK_PIXEL = new Color (0, 0, 0f, 1f);
	public static readonly Color PATH_PIXEL = new Color (235f / 255f, 137f / 255f, 49f / 255f, 1f);
	public static readonly Color ENTRANCE_PIXEL = new Color (1f, 1f, 1f, 1f);
	public static readonly Color FOREST_PIXEL = new Color (163f / 255f, 206f / 255f, 39f / 255f, 1f);
	public static readonly Color FOREST_PIXEL_2 = new Color (68f / 255f, 137f / 255f, 26f / 255f, 1f);
	
	// Use this for initialization
	void Start ()
	{
		randomiser = new SeededRandomiser (12345678);
	
		CreateForest();
		CreateMap ();
		CreatePathQuads(map);
		CreateForestAreas (map);
		CreatePlayer (map);
	}
	
	void CreateForest () {
		paths = new GameObject ();
		paths.name = "paths";
		paths.transform.parent = transform;
		
		forest = new GameObject ();
		forest.name = "forest";
		forest.transform.parent = transform;
		
		Vector3 pos = Vector3.zero;
		pos.y = 0.5f;
		forest.transform.localPosition = pos;		
	}
	
	void CreateMap () {
	
		MapTag[] tags = {new MapTag(PATH_PIXEL, "path"), 
			new MapTag(ENTRANCE_PIXEL, "entrance"),
			new MapTag(FOREST_PIXEL, "forest"),
			new MapTag(FOREST_PIXEL_2, "forest")};
			
		map = new Map(pathMap, new MapTag(BLANK_PIXEL, "empty"), tags, hilliness);
		MapUtils.BuildHeightMap(map, randomiser);
	}
	
	void CreatePathQuads (Map map)
	{
		Vector3[] vertices = new Vector3[4];
		vertices [0] = new Vector3 (0.5f, 0, 0.5f);
		vertices [1] = new Vector3 (-0.5f, 0, 0.5f);
		vertices [2] = new Vector3 (0.5f, 0, -0.5f);
		vertices [3] = new Vector3 (-0.5f, 0, -0.5f);
		Vector3 position = Vector3.zero;
		
		int depth = map.mapDepth;
		int width = map.mapWidth;
		for (int z = 0; z < depth; z++) {
			for (int x = 0; x < width; x++) {
			
				MapLocation location = map.GetMapLocationForPosition (x, z);
				if (!location.tag.Equals("path") &&
				    !location.tag.Equals("entrance")) continue;
				for (int i = 0; i < 4; i++) {
					vertices[i].y = location.corners[i];
				}
				position.x = x;
				position.y = 0.5f;
				position.z = z;
				
				if (location == null) continue;
				GameObject quad = MeshUtils.CreatePlane ("path", vertices, pathMaterial, false);
				quad.transform.localPosition = position;
				quad.transform.parent = paths.transform;
				location.quad = quad;
			}
		}
	}
	
	void CreateForestAreas (Map map)
	{
		int depth = map.mapDepth;
		int width = map.mapWidth;
		
		forestAreas = new ArrayList ();
		for (int z = 0; z < depth; z++) {
			for (int x = 0; x < width; x++) {
				Color pixel = map.GetPixelForLocation (x, z);
				Color currentPixel = pixel;
				if ((pixel.Equals (FOREST_PIXEL) || pixel.Equals (FOREST_PIXEL_2)) && !InForestArea (forestAreas, x, z)) {
					GameObject forestGO = new GameObject ();
					forestGO.name = "forestArea";
					forestGO.transform.parent = forest.transform;
					ForestArea forestArea = forestGO.AddComponent ("ForestArea") as ForestArea;
					int areaWidth = 0;
					int areaHeight = 0;
					while (pixel.Equals(currentPixel)) {
						areaHeight++;
						pixel = map.GetPixelForLocation (x, z + areaHeight);
					}
					pixel = map.GetPixelForLocation (x, z);
					while (pixel.Equals(currentPixel)) {
						areaWidth++;
						pixel = map.GetPixelForLocation (x + areaWidth, z);
					}
					
					forestArea.rect = new Rect(x, z, areaWidth, areaHeight);
					forestArea.Ground = CreateGround (x - 0.5f, z - 0.5f, areaWidth, areaHeight, map.locations, groundMaterial);
					forestArea.AddForestObjects(CreateQuads ("trees", forestArea.rect, 3f, 0.65f, map.locations, 0.9f, randomiser, treeMaterial));
					forestArea.AddForestObjects(CreateQuads ("bushes", forestArea.rect, 0.25f, 0.4f, map.locations, 0.8f, randomiser, bushMaterial));
					
					forestAreas.Add (forestGO);
				}
			}
		}
	}
	
	bool InForestArea (ArrayList forestAreas, int x, int z)
	{
		foreach (GameObject area in forestAreas) {
			ForestArea maker = area.GetComponent<ForestArea> ();
			if (maker.Contains (x, z))
				return true;
		}
		
		return false;
	}
	
	void CreatePlayer (Map map)
	{
		MapLocation entrance = (MapLocation)map.entrances [0];
		GameObject player = (GameObject)Instantiate (playerPrefab, Vector3.zero, Quaternion.identity);
		Vector3 playerPosition = new Vector3 (entrance.x, entrance.y + 0.5f + (player.transform.localScale.y / 2), entrance.z);
		player.transform.position = playerPosition;
		PlayerControl control = player.GetComponent<PlayerControl> ();
		control.paths = this;
		
		SmoothCameraTracker tracker = playerCamera.GetComponent<SmoothCameraTracker> ();
		tracker.target = player;
	}
	
	void SetPointValues (Vector3[] points, int x, int z, MapLocation otherLocation)
	{
		points [0].x = 0;
		points [1].x = 0;
		points [2].x = 0;
		points [3].x = 0;
		points [0].z = 0;
		points [1].z = 0;
		points [2].z = 0;
		points [3].z = 0;
		// 0:bottom-left, 1:bottom-right, 2:top-left, 3:top-right
		if (otherLocation.x < x) {
			points [0].x = -0.5f;
			points [2].x = -0.5f;
		} else if (otherLocation.x > x) {
			points [1].x = 0.5f;
			points [3].x = 0.5f;
		} else if (otherLocation.z < z) {
			points [0].z = -0.5f;
			points [2].z = -0.5f;
		} else if (otherLocation.z > z) {
			points [1].z = 0.5f;
			points [3].z = 0.5f;
		}
	}
	
	public MapLocation GetTransitLocationForMove (Vector3 position, Vector3 move, bool rotated)
	{
		if (!rotated) {
			MapLocation xLocation = map.GetXLocation (position, move);
			if (xLocation != null)
				return xLocation;
			return null;
		} else {
			MapLocation zLocation = map.GetZLocation (position, move);
			if (zLocation != null)
				return zLocation;
		}
		return null;
	}
	
	public GameObject CreateGround (float x, float z, float width, float depth, MapLocation[,] locations, Material material) {
		GameObject ground = new GameObject();
		Vector3 pos = ground.transform.localPosition;
		pos.x = x;
		pos.z = z;
		ground.transform.localPosition = pos;
		
		Vector3[] vertices = new Vector3[4];
		vertices[0] = new Vector3(0.5f, 0, 0.5f);
		vertices[1] = new Vector3(-0.5f, 0, 0.5f);
		vertices[2] = new Vector3(0.5f, 0, -0.5f);
		vertices[3] = new Vector3(-0.5f, 0, -0.5f);
		Vector3 quadPos = Vector3.zero;
		quadPos.y = 0.5f;
		for (int qz = 0; qz < depth; qz++) {
			for (int qx = 0; qx < width; qx++) {
				
				int hx = Mathf.RoundToInt(x + 0.5f) + qx;
				int hz = Mathf.RoundToInt(z + 0.5f) + qz;
				MapLocation location = locations[hx, hz];
				for (int i = 0; i < 4; i++) {
					vertices[i].y = location.corners[i];
				}
				
				GameObject quad = MeshUtils.CreatePlane("ground", vertices, material, false);
				quadPos.x = x + qx + 0.5f;
				quadPos.z = z + qz + 0.5f;
				quad.transform.localPosition = quadPos;
				quad.transform.parent = ground.transform;
				
				location.quad = quad;
			}
		}
		//Vector3 scale = ground.transform.localScale;
		//scale.x = width;
		//scale.z = height;
		//ground.transform.localScale = scale;
		
		//MeshRenderer renderer = ground.GetComponent<MeshRenderer>();
		//renderer.material = material;
		//renderer.material.SetTextureScale("_MainTex", new Vector2(width, height));
		
		return ground;
	}
	
	public GameObject CreateQuads (string name, Rect rect, float scale, float density, MapLocation[,] locations, float minimumDistanceBetween, SeededRandomiser randomiser, Material material) {
		
		GameObject go = new GameObject();
		go.name = name;
		
		Vector3 holderPos = go.transform.localPosition;
		holderPos.x = rect.x;
		holderPos.y = 0.5f;
		holderPos.z = rect.y;
		go.transform.localPosition = holderPos;
		
		ForestObjects forestObjects = (ForestObjects)go.AddComponent(typeof(ForestObjects));
		int totalTrees = Mathf.RoundToInt(rect.width * rect.height * density);
		
		GameObject[] xAxis = new GameObject[totalTrees];
		GameObject[] zAxis = new GameObject[totalTrees];
		int timeoutMax = 100;
		int timeout = timeoutMax;
		int treesMade = 0;
		minimumDistanceBetween *= minimumDistanceBetween;
		while (treesMade < totalTrees && timeout > 0) {
			float xPosition = randomiser.GetRandomFromRange(0, rect.xMax - rect.xMin);
			float zPosition = randomiser.GetRandomFromRange(0, rect.yMax - rect.yMin);
			int locationX = Mathf.FloorToInt(rect.x + xPosition);
			int locationZ = Mathf.FloorToInt(rect.y + zPosition);
			float yPosition = (scale / 2) + GetLowestYForLocation(locations[locationX, locationZ]);
			Vector3 pos = new Vector3(xPosition, yPosition, zPosition);
			if (IsValidPosition(pos, xAxis, treesMade, minimumDistanceBetween)) {	
				timeout = timeoutMax;
				
				GameObject plane1 = GameObject.CreatePrimitive(PrimitiveType.Quad);
				MeshRenderer renderer = plane1.GetComponent<MeshRenderer>();
				renderer.material = material;
				plane1.transform.parent = go.transform;
				plane1.transform.localScale = new Vector3(scale, scale, scale);
				plane1.transform.localPosition = pos;
				
				GameObject plane2 = GameObject.CreatePrimitive(PrimitiveType.Quad);
				renderer = plane2.GetComponent<MeshRenderer>();
				renderer.material = material;
				plane2.transform.parent = go.transform;
				plane2.transform.localEulerAngles = new Vector3(0, 90, 0);
				plane2.transform.localScale = new Vector3(scale, scale, scale);
				plane2.transform.localPosition = pos;
				
				xAxis[treesMade] = plane1;
				zAxis[treesMade++] = plane2;
			} else {
				timeout--;
			}
		}
		
		forestObjects.xAxis = xAxis;
		forestObjects.zAxis = zAxis;
		
		return go;
	}
	
	float GetLowestYForLocation (MapLocation location) {
		float min = int.MaxValue;
		foreach (float corner in location.corners) {
			if (corner < min) min = corner;
		}
		
		return min;
	}
	
	bool IsValidPosition (Vector3 position, GameObject[] trees, int maxTree, float minDist) {
		for (int i = 0; i < maxTree; i++) {
			GameObject tree = trees[i];
			Vector3 treePos = tree.transform.localPosition;
			float xDist = (treePos.x - position.x);
			float zDist = (treePos.z - position.z);
			float dist = (xDist * xDist) + (zDist * zDist);
			
			if (dist < minDist) return false;
		}
		
		return true;
	}
	
	
	public void UpdateForestAreasInFrontOfPosition (int x, int z, int rotation)
	{
		foreach (GameObject area in forestAreas) {
			ForestArea maker = area.GetComponent<ForestArea> ();
			maker.UpdateForPosition (x, z, rotation);
		}
	}
	
	public MapLocation GetPerpendicularTransitLocationForMove (Vector3 position, Vector3 move, bool rotated)
	{
		if (!rotated) {
			MapLocation zLocation = map.GetZLocation (position, move);
			if (zLocation != null)
				return zLocation;
			return null;
		} else {
			MapLocation xLocation = map.GetXLocation (position, move);
			if (xLocation != null)
				return xLocation;
		}
		return null;
	}
	
	public float GetYForPosition (Vector3 position, bool rotated) {
		return map.GetPositionHeight(position.x, position.z, rotated);
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}
