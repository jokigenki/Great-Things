using UnityEngine;
using System.Collections;

public class Forest : MonoBehaviour
{
	public ColourMaterialMap treeMaterialMap;
	public ColourMaterialMap bushMaterialMap;
	public ColourMaterialMap rockMaterialMap;
	public ColourMaterialMap groundMaterialMap;
	public ColourMaterialMap waterMaterialMap;
	public ColourMaterialMap pathMaterialMap;
	
	public Texture2D pathMap;
	public GameObject player;
	public Camera playerCamera;
	public GameObject floorPrefab;
	public int hilliness = 2;
	public int drawDistance = 10;
	
	public GameObject forestPrefab;
	
	[HideInInspector]
	public bool ready;

	GameObject forest;
	GameObject paths;
	
	ArrayList forestAreas;
	SeededRandomiser randomiser;
	Map map;
	Vector3 lastUpdatePosition;
	
	public static readonly Color BLANK_PIXEL = new Color (0, 0, 0f, 1f);
	public static readonly Color PATH_PIXEL = new Color (235f / 255f, 137f / 255f, 49f / 255f, 1f);
	public static readonly Color ENTRANCE_PIXEL = new Color (1f, 1f, 1f, 1f);
	public static readonly Color FOREST_PIXEL = new Color (163f / 255f, 206f / 255f, 39f / 255f, 1f);
	public static readonly Color FOREST_PIXEL_2 = new Color (68f / 255f, 137f / 255f, 26f / 255f, 1f);
	public static readonly Color POOL = new Color (0, 179f / 255f, 224f / 255f, 1f);
	
	// Use this for initialization
	void Start ()
	{
		randomiser = new SeededRandomiser (12345678);
	
		CreateForestParentObjects();
		CreateMap ();
		CreatePlayer (map);
		CreatePathQuads(map);
		StartCoroutine(CreateForestAreas (map));
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	void CreateForestParentObjects () {
		paths = new GameObject ();
		paths.name = "paths";
		paths.transform.parent = transform;
		
		forest = new GameObject ();
		forest.name = "forest";
		forest.transform.parent = transform;
		
		Vector3 pos = Vector3.zero;
		pos.y = 0.5f;
		forest.transform.localPosition = pos;		
		
		forestAreas = new ArrayList ();
	}
	
	void CreateMap () {
	
		MapTag[] tags = {new MapTag(PATH_PIXEL, "path"), 
			new MapTag(ENTRANCE_PIXEL, "entrance"),
			new MapTag(FOREST_PIXEL, "forest"),
			new MapTag(FOREST_PIXEL_2, "forest"),
			new MapTag(POOL, "pool")};
			
		map = new Map(pathMap, new MapTag(BLANK_PIXEL, "empty"), tags, hilliness);
		MapUtils.SetHeightsForMap(map, randomiser);
	}
	
	// Creates the paths for the map. Each square is a separate object.
	// TODO: if we merge these, do we get better performance?
	// TODO: Can we add more geometry? 
	// TODO: paths need to have better edges, so they are more clearly marked as paths, and so they can merge into the forest more
	void CreatePathQuads (Map map)
	{
		Vector3[] vertices = GetVerticesForFlatQuad();
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
				for (int i = 0; i < 4; i++) {
					GameObject quad = MeshUtils.CreatePlane ("path", vertices, pathMaterialMap.getMaterial(randomiser), false);
					quad.transform.localPosition = position;
					quad.transform.parent = paths.transform;
					location.quad = quad;
				}
			}
		}
	}
	
	// Returns a set of vertices for a basic quad
	public Vector3[] GetVerticesForFlatQuad () {
		Vector3[] vertices = new Vector3[4];
		vertices [0] = new Vector3 (0.5f, 0, 0.5f);
		vertices [1] = new Vector3 (-0.5f, 0, 0.5f);
		vertices [2] = new Vector3 (0.5f, 0, -0.5f);
		vertices [3] = new Vector3 (-0.5f, 0, -0.5f);
		
		return vertices;
	}
	
	// TODO: Given a colour should return the correct material
	Material GetMaterialForColour (Color colour) {
		return null;
	}
	
	// If the given area has been created and stored in forestAreas, does nothing
	// Iterates the groundMaterialMap, and if it finds the given colour, returns true.
	// if the given colour is not in the groundMaterialMap, it will return false
	bool ShouldCreateAForestArea (Color pixel, int x, int z) {
		
		if (InForestArea (forestAreas, x, z, true)) return false;
		foreach (Color color in groundMaterialMap.colors) {
			if (pixel.Equals(color)) return true;
		}
		
		return false;
	}
	
	// distance is in map pixels
	// Iterates through each pixel in the colour map and creates each of the rectangular forest areas
	// This is called as a coroutine, and builds 10 sections of the map before it yields
	IEnumerator CreateForestAreas (Map map)
	{	
		for (int z = 0; z < map.mapDepth; z++) {
			for (int x = 0; x < map.mapWidth; x++) {
				Color pixel = map.GetPixelForLocation (x, z);
				Color currentPixel = pixel;
				if (ShouldCreateAForestArea(pixel, x, z)) {
					//print ("create forest area:" + x + ", " + z);
					GameObject forestGO = new GameObject ();
					forestGO.name = "forestArea";
					forestGO.transform.parent = forest.transform;
					ForestArea forestArea = forestGO.AddComponent <ForestArea>() as ForestArea;
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
					CreateForestAreaObjects(forestArea, x, z, areaWidth, areaHeight);
					forestAreas.Add (forestGO);
					
					forestArea.RendererEnabled = false;
					yield return null;
				}
			}
		}
		
		ready = true;
		
		Renderer renderer = player.GetComponent<Renderer>();
		renderer.enabled = true;
	}
	
	// Creates the trees, bushes, rocks etc. for the given forestArea
	void CreateForestAreaObjects (ForestArea forestArea, int x, int z, int areaWidth, int areaHeight) {
		forestArea.Ground = CreateGround (x - 0.5f, z - 0.5f, areaWidth, areaHeight, map.locations, groundMaterialMap.getMaterial(randomiser));
		forestArea.AddForestObjects(CreateForestObjects ("trees", forestArea.rect, 4f, 0.5f, map.locations, 1.1f, randomiser, treeMaterialMap));
		forestArea.AddForestObjects(CreateForestObjects ("bushes", forestArea.rect, 0.25f, 0.9f, map.locations, 0.4f, randomiser, bushMaterialMap));
		forestArea.AddForestObjects(CreateForestObjects ("rocks", forestArea.rect, 0.2f, 1.5f, map.locations, 0.2f, randomiser, rockMaterialMap));
	}

	// Is the given x, z value inside the existing forestAreas
	// Can be used to switch on the renderer of the forestArea if it is disabled
	bool InForestArea (ArrayList forestAreas, int x, int z, bool enableRendererIfDisabled)
	{
		foreach (GameObject area in forestAreas) {
			ForestArea maker = area.GetComponent<ForestArea> ();
			if (maker.Contains (x, z)) {
				if (enableRendererIfDisabled && !maker.RendererEnabled) maker.RendererEnabled = true;
				return true;
			}
		}
		
		return false;
	}
	
	// Create the player avatar at the map entrance
	// TODO: expand this so we can move the player to various entrances
	void CreatePlayer (Map map)
	{
		MapLocation entrance = (MapLocation)map.entrances [0];
		player.SetActive(true);
		Vector3 playerPosition = new Vector3 (entrance.x, entrance.y + 0.5f + (player.transform.localScale.y / 2), entrance.z);
		player.transform.position = playerPosition;
		Renderer renderer = player.GetComponent<Renderer>();
		renderer.enabled = false;
	}
	
	// Create a ground object for a forestArea, by creating a quad for each pixel in the map
	// The y position of each corner in the quad is taken from the pregenerated locations map
	// Once these are created, they are merged into a single object
	public GameObject CreateGround (float x, float z, float width, float depth, MapLocation[,] locations, Material material) {
		Vector3[] vertices = GetVerticesForFlatQuad();
		Vector3 quadPos = Vector3.zero;
		quadPos.y = 0.5f;
		GameObject[] quads = new GameObject[Mathf.RoundToInt(depth * width)];
		int j = 0;
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
				
				location.quad = quad;
				quads[j++] = quad;
			}
		}
		
		return CreateCombinedGroundObject(quads, material);
	}
	
	// Takes an array of ground quads and turns them into a single mesh
	public GameObject CreateCombinedGroundObject (GameObject[] quads, Material material) {
		GameObject ground = new GameObject();
		ground.AddComponent(typeof(MeshFilter));
		MeshRenderer mr = (MeshRenderer)ground.AddComponent(typeof(MeshRenderer));
		mr.material = material;
		
		CombineInstance[] combine = new CombineInstance[quads.Length];
		int k = 0;
		while (k < quads.Length) {
			GameObject quad = quads[k];
			quad.transform.parent = ground.transform;
			MeshFilter mesh = quad.GetComponent<MeshFilter>();
			combine[k].mesh = mesh.sharedMesh;
			combine[k].transform = mesh.transform.localToWorldMatrix;
			Destroy(mesh.gameObject);
			k++;
		}
		ground.transform.GetComponent<MeshFilter>().mesh = new Mesh();
		ground.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
		ground.transform.gameObject.SetActive(true);
		
		return ground;
	}
	
	// Creates forest objects inside the given rectangle
	public GameObject CreateForestObjects (string name, Rect rect, float scale, float density, MapLocation[,] locations, float minimumDistanceBetween, SeededRandomiser randomiser, ColourMaterialMap materialMap) {
		
		GameObject go = new GameObject();
		go.name = name;
		
		Vector3 holderPos = go.transform.localPosition;
		holderPos.x = rect.x;
		holderPos.y = 0.5f;
		holderPos.z = rect.y;
		go.transform.localPosition = holderPos;
		
		ForestObjects forestObjects = (ForestObjects)go.AddComponent(typeof(ForestObjects));
		forestObjects.materialMap = materialMap;
		forestObjects.randomiser = randomiser;
		int totalQuads = Mathf.RoundToInt(rect.width * rect.height * density);
		
		ArrayList xAxisArray = new ArrayList();
		ArrayList zAxisArray = new ArrayList();
		int timeoutMax = 100;
		int timeout = timeoutMax;
		int objectsMade = 0;
		minimumDistanceBetween *= minimumDistanceBetween;
		while (objectsMade < totalQuads && timeout > 0) {
			float xPosition = randomiser.GetRandomFromRange(0, rect.xMax - rect.xMin - 0.5f);
			float zPosition = randomiser.GetRandomFromRange(0, rect.yMax - rect.yMin - 0.5f);
			int locationX = Mathf.FloorToInt(rect.x + xPosition);
			int locationZ = Mathf.FloorToInt(rect.y + zPosition);
			float yPosition = (scale / 2) + GetLowestYForLocation(locations[locationX, locationZ]);
			Vector3 pos = new Vector3(xPosition, yPosition, zPosition);
			if (IsValidPosition(pos, xAxisArray, objectsMade, minimumDistanceBetween)) {	
				CreateDoubleSidedForestObjectQuad(go, materialMap, scale, pos, xAxisArray, zAxisArray);
				timeout = timeoutMax;
				objectsMade++;
			} else {
				timeout--;
			}
		}
		
		forestObjects.SetXAxis(go, ListToArray(xAxisArray));
		forestObjects.SetZAxis(go, ListToArray(zAxisArray));
		
		return go;
	}
	
	// Creates 4 quads, each rotated by 90º from 45º
	// Each quad is assigned a random material from the given MaterialMap
	void CreateDoubleSidedForestObjectQuad (GameObject parent, ColourMaterialMap materialMap, float scale, Vector3 pos, ArrayList xAxisArray, ArrayList zAxisArray) {
		for (int rotation = 45; rotation < 360; rotation+=90) {
			GameObject plane = CreateForestObjectQuad(materialMap.getMaterial(randomiser));
			plane.transform.parent = parent.transform;
			plane.transform.localScale = new Vector3(scale, scale, scale);
			plane.transform.localPosition = pos;
			plane.transform.localEulerAngles = new Vector3(0, rotation, 0);
			
			ArrayList axisArray = rotation == 45 || rotation == 225 ? xAxisArray : zAxisArray;
			axisArray.Add(plane);
		}
	}
	
	// Turns an ArrayList into an Array of GameObjects
	GameObject[] ListToArray (ArrayList list) {
		GameObject[] arr = new GameObject[list.Count];
		int i = 0;
		foreach (GameObject obj in list) {
			arr[i++] = obj;
		}
		
		return arr;
	}
	
	// Creates a vertically orientated quad
	GameObject CreateForestObjectQuad (Material material) {
		GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
		MeshRenderer renderer = quad.GetComponent<MeshRenderer>();
		renderer.material = material;
		renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		
		return quad;
	}

	// Given a location, returns the lowest Y value of the corners of that location
	float GetLowestYForLocation (MapLocation location) {
		float min = int.MaxValue;
		foreach (float corner in location.corners) {
			if (corner < min) min = corner;
		}
		
		return min;
	}
	
	// Given a position, returns whether it is the minimum distance away from each object in the existingObjects list or not
	bool IsValidPosition (Vector3 position, ArrayList existingObjects, int max, float minDist) {
		for (int i = 0; i < max; i++) {
			GameObject existing = (GameObject)existingObjects[i];
			Vector3 pos = existing.transform.localPosition;
			float xDist = (pos.x - position.x);
			float zDist = (pos.z - position.z);
			float dist = (xDist * xDist) + (zDist * zDist);
			
			if (dist < minDist) return false;
		}
		
		return true;
	}
	
	// turn the forest areas renderers off if they are outside the render area
	public void UpdateForestAreaVisibility (int x, int z, int rotation) {
		if (!ready || UpdateHasNotChanged(x, z, rotation)) return;
		Rect distRect = new Rect(x - drawDistance, z - drawDistance, drawDistance * 2, drawDistance * 2); 
		foreach (GameObject area in forestAreas) {
			ForestArea maker = area.GetComponent<ForestArea> ();
			
			maker.RendererEnabled =  maker.Overlaps(distRect);
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
		return rotated ? map.GetZLocation (position, move) : map.GetXLocation (position, move);
	}
	
	// Given a position, returns the y position at that map location
	public float GetYForPosition (Vector3 position, bool rotated) {
		return map.GetPositionHeight(position.x, position.z, rotated);
	}
}
