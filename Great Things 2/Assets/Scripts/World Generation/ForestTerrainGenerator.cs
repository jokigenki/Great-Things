using UnityEngine;
using System.Collections;

public class ForestTerrainGenerator : MonoBehaviour
{
	public Material treeMaterial;
	public Material bushMaterial;
	public Material rockMaterial;
	public Material groundMaterial;
	public Texture2D pathMap;
	public GameObject player;
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
					forestArea.AddForestObjects(CreateQuads ("trees", forestGO, forestArea.rect, 3.5f, 0.5f, map.locations, 1.1f, randomiser, treeMaterial));
					forestArea.AddForestObjects(CreateQuads ("bushes", forestGO, forestArea.rect, 0.25f, 0.9f, map.locations, 0.4f, randomiser, bushMaterial));
					forestArea.AddForestObjects(CreateQuads ("rocks", forestGO, forestArea.rect, 0.2f, 1.5f, map.locations, 0.2f, randomiser, rockMaterial));
					
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
		//GameObject player = (GameObject)Instantiate (playerPrefab, Vector3.zero, Quaternion.identity);
		player.SetActive(true);
		Vector3 playerPosition = new Vector3 (entrance.x, entrance.y + 0.5f + (player.transform.localScale.y / 2), entrance.z);
		player.transform.position = playerPosition;
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
		if (map == null) return null;
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
	
	public GameObject CreateQuads (string name, GameObject parent, Rect rect, float scale, float density, MapLocation[,] locations, float minimumDistanceBetween, SeededRandomiser randomiser, Material material) {
		
		GameObject go = new GameObject();
		go.name = name;
		go.transform.parent = parent.transform;
		
		Vector3 holderPos = go.transform.localPosition;
		holderPos.x = rect.x;
		holderPos.y = 0.5f;
		holderPos.z = rect.y;
		go.transform.localPosition = holderPos;
		
		ForestObjects forestObjects = (ForestObjects)go.AddComponent(typeof(ForestObjects));
		int totalQuads = Mathf.RoundToInt(rect.width * rect.height * density);
		
		ArrayList xAxisArray = new ArrayList();
		ArrayList zAxisArray = new ArrayList();
		int timeoutMax = 100;
		int timeout = timeoutMax;
		int treesMade = 0;
		minimumDistanceBetween *= minimumDistanceBetween;
		while (treesMade < totalQuads && timeout > 0) {
			float xPosition = randomiser.GetRandomFromRange(0, rect.xMax - rect.xMin - 0.5f);
			float zPosition = randomiser.GetRandomFromRange(0, rect.yMax - rect.yMin - 0.5f);
			int locationX = Mathf.FloorToInt(rect.x + xPosition);
			int locationZ = Mathf.FloorToInt(rect.y + zPosition);
			float yPosition = (scale / 2) + GetLowestYForLocation(locations[locationX, locationZ]);
			Vector3 pos = new Vector3(xPosition, yPosition, zPosition);
			if (IsValidPosition(pos, xAxisArray, treesMade, minimumDistanceBetween)) {	
				timeout = timeoutMax;
				
				GameObject planeX = CreateQuad(go, material, scale, pos, 0, 0, 1, 1);
				GameObject planeZ = CreateQuad(go, material, scale, pos, 0, 0, 1, 1);
				planeZ.transform.localEulerAngles = new Vector3(0, 90, 0);
				
				xAxisArray.Add(planeX);
				zAxisArray.Add(planeZ);
				treesMade++;
			} else {
				timeout--;
			}
		}
		
		forestObjects.SetXAxis(go, ListToArray(xAxisArray), material);
		forestObjects.SetZAxis(go, ListToArray(zAxisArray), material);
		//forestObjects.XAxis = ListToArray(xAxisArray);
		//forestObjects.ZAxis = ListToArray(zAxisArray);
		
		return go;
	}
	
	GameObject[] ListToArray (ArrayList list) {
		GameObject[] arr = new GameObject[list.Count];
		int i = 0;
		foreach (GameObject obj in list) {
			arr[i++] = obj;
		}
		
		return arr;
	}
	
	GameObject CreateQuad (GameObject parent, Material material, float scale, Vector3 pos, float uvLeft, float uvTop, float uvRight, float uvBottom) { 
			GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
		renderer.material = material;
		quad.transform.parent = parent.transform;
		quad.transform.localScale = new Vector3(scale, scale, scale);
		quad.transform.localPosition = pos;
		//Mesh mesh = quad.GetComponent<MeshFilter>().mesh;
		//mesh.uv = new Vector2[] { new Vector2(uvLeft, uvTop), new Vector2(uvRight, uvBottom), new Vector2(uvRight, uvTop), new Vector2(uvLeft, uvBottom)};
		
		return quad;
	}
	
	float GetLowestYForLocation (MapLocation location) {
		float min = int.MaxValue;
		foreach (float corner in location.corners) {
			if (corner < min) min = corner;
		}
		
		return min;
	}
	
	bool IsValidPosition (Vector3 position, ArrayList objects, int maxTree, float minDist) {
		for (int i = 0; i < maxTree; i++) {
			GameObject tree = (GameObject)objects[i];
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
