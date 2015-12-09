using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class Forest : MonoBehaviour
{
	public ColourMaterialMap treeMaterialMap;
	public ColourMaterialMap bushMaterialMap;
	public ColourMaterialMap rockMaterialMap;
	public ColourMaterialMap groundMaterialMap;
	public ColourMaterialMap waterMaterialMap;
	public ColourMaterialMap pathMaterialMap;
	public List<Color> poolLightColours;
	public List<Color> forestLightColours;
	public GameObject blobShadowPrefab;
	
	public Texture2D pathMap;
	public GameObject player;
	public Camera playerCamera;
	public GameObject floorPrefab;
	public int hilliness = 2;
	public int drawDistance = 10;
	public float pathInsetAmount = 0.05f;
	
	public GameObject forestPrefab;
	
	[HideInInspector]
	public bool ready;

	GameObject forest;
	GameObject paths;
	Vector3 pathInset;
	List<GameObject> forestAreas;
	SeededRandomiser randomiser;
	Map map;
	Vector3 lastUpdatePosition;
	
	Queue<Func<IEnumerator>> generationQueue = new Queue<Func<IEnumerator>>();
	
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
		pathInset = new Vector3(0, pathInsetAmount, 0);
		
		CreateForestParentObjects();
		CreateMap();
		CreatePlayer(map);
		
		StartCoroutine(StartGeneratorQueue());
	}
	
	IEnumerator StartGeneratorQueue () {
		generationQueue.Enqueue(()=>CreatePath(map));
		generationQueue.Enqueue(()=>CreateForestAreas(map));
		generationQueue.Enqueue(CompleteSetup);
		
		while(true)
		{
			if(generationQueue.Count > 0)
			{
				yield return StartCoroutine(generationQueue.Dequeue()());
			}
			else yield return null;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	void CreateForestParentObjects () {
		forest = new GameObject ();
		forest.name = "forest";
		forest.transform.parent = transform;
		
		Vector3 pos = Vector3.zero;
		pos.y = 0.5f;
		forest.transform.localPosition = pos;		
		
		forestAreas = new List<GameObject>();
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
	
	// Creates the paths for the map. Each square is a separate object, which it then merged into a single object.
	IEnumerator CreatePath (Map map)
	{
		Vector3[] vertices = GetVerticesForFlatQuad();
		Vector3 position = Vector3.zero;
		
		Material material = pathMaterialMap.getMaterial(randomiser);
		Material underMaterial = groundMaterialMap.getMaterial(randomiser);
		List<GameObject> quads = new List<GameObject>();
		List<GameObject> underQuads = new List<GameObject>();
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
				
				BuildPathQuad(quads, position, vertices, material);
				int[] vertexIndicesIn = new int[] {0, 2, 3, 1, 0};
				int[] vertexIndicesOut = new int[] {0, 1, 3, 2, 0};
				int[] offsetsIn = new int[] {1, 0, 0, -1, -1, 0, 0, 1};
				int[] offsetsOut = new int[] {0, 1, -1, 0, 0, -1, 1, 0};
				BuildVerticalQuads (underQuads, position, vertices, underMaterial, vertexIndicesIn, offsetsIn, false);
				BuildVerticalQuads (underQuads, position, vertices, underMaterial, vertexIndicesOut, offsetsOut, true);
			}
		}
		
		GameObject paths = MeshUtils.CombineQuads("paths", quads.ToArray(), material, true);
		paths.transform.parent = transform;
		paths.transform.position = paths.transform.position - pathInset;
		paths.AddComponent(typeof(MeshCollider));	
		
		GameObject underPaths = MeshUtils.CombineQuads("underPaths", underQuads.ToArray(), underMaterial, true);
		underPaths.transform.parent = transform;
		
		yield return null;
	}
	
	public void BuildPathQuad (List<GameObject> target, Vector3 position, Vector3[] vertices, Material material) {
		GameObject quad = MeshUtils.CreatePlane ("path", vertices, material, false);	
		//GameObject subdividedQuad = MeshUtils.CombineQuads("path", MeshUtils.SubdivideQuad("path", quad, material, false), material);
		quad.transform.localPosition = position;
		target.Add(quad);
	}
	
	public void BuildVerticalQuads (List<GameObject> target, Vector3 position, Vector3[] vertexBase, Material material, int[] vertexIndices, int[] offsets, bool alwaysOffset) {
		for (int i = 0; i < vertexIndices.Length - 1; i++) {
			GameObject quad = MeshUtils.CreatePlane ("pathN", GetVerticesForVerticalQuad(vertexBase, vertexIndices[i], vertexIndices[i + 1]), material, false);
			if (alwaysOffset || ShouldOffsetUnderGroundQuad(position, offsets[i * 2], offsets[(i * 2) + 1])) {
				quad.transform.localPosition = position - pathInset;
			} else {
				quad.transform.localPosition = position;
			}
			target.Add(quad);
		}
	}
	
	public bool ShouldOffsetUnderGroundQuad(Vector3 position, int offsetX, int offsetZ) {
		Vector3 checkPos = new Vector3(position.x + offsetX, position.y, position.z + offsetZ);
		return LocationIsPath(checkPos);
	}
	
	public bool LocationIsPath (Vector3 position) {
		MapLocation location = map.GetMapLocationForPosition(position);
		if (location == null) return false;
		return !location.tag.Equals("forest");
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
	
	public Vector3[] GetVerticesForVerticalQuad (Vector3[] vertexBase, int first, int second) {
		Vector3[] vertices = new Vector3[4];
		vertices[2] = vertexBase[first];
		vertices[3] = vertexBase[second];
		vertices[0] = new Vector3(vertexBase[first].x, -2, vertexBase[first].z);
		vertices[1] = new Vector3(vertexBase[second].x, -2, vertexBase[second].z);
					
		return vertices;
	}
	
	// TODO: Given a colour should return the correct material
	Material GetMaterialForColour (Color colour) {
		return null;
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
				if (ShouldCreateAForestArea(pixel, x, z, groundMaterialMap)) {
					CreateForestArea(pixel, currentPixel, x, z);
					yield return null;
				} else if (ShouldCreateAForestArea(pixel, x, z, waterMaterialMap)) {
					CreatePoolArea(pixel, currentPixel, x, z);
					yield return null;
				}
			}
		}
	}
	
	// If the given area has been created and stored in forestAreas, does nothing
	// Iterates the groundMaterialMap, and if it finds the given colour, returns true.
	// if the given colour is not in the groundMaterialMap, it will return false
	bool ShouldCreateAForestArea (Color pixel, int x, int z, ColourMaterialMap matMap) {
		
		if (InForestArea (forestAreas, x, z, true)) return false;
		foreach (Color color in matMap.colors) {
			if (pixel.Equals(color)) return true;
		}
		
		return false;
	}
	
	IEnumerator CompleteSetup() {
		ready = true;
		
		Renderer renderer = player.GetComponent<Renderer>();
		renderer.enabled = true;

		PrefabUtils.GenerateNestedPrefab(gameObject, "Meshes", "Prefabs");
		
		yield return null;
	}
	
	public void CreateForestArea (Color pixel, Color currentPixel, int x, int z) {
		GameObject forestGO = new GameObject ();
		forestGO.name = "forestArea";
		forestGO.transform.parent = forest.transform;
		ForestArea forestArea = forestGO.AddComponent <ForestArea>() as ForestArea;
		int areaWidth = 0;
		forestArea.type = "forest";
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
		
		int index = randomiser.GetRandomIntFromRange(0, forestLightColours.Count - 1);
		Color colour = forestLightColours[index];
		CreateLightForArea(forestGO, x, z, (float)areaWidth, (float)areaHeight, colour, 4);
	}
	
	public void CreatePoolArea (Color pixel, Color currentPixel, int x, int z) {
		GameObject poolGO = new GameObject ();
		poolGO.name = "poolArea";
		poolGO.transform.parent = forest.transform;
		ForestArea forestArea = poolGO.AddComponent <ForestArea>() as ForestArea;
		forestArea.type = "pool";
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
		forestArea.Ground = CreateWater (x - 0.5f, z - 0.5f, areaWidth, areaHeight, map.locations, waterMaterialMap.getMaterial(randomiser));
		MeshUtils.JitterMeshOnY(forestArea.Ground, -10, 10, 0.01f, -0.1f, 0.1f);
		forestAreas.Add (poolGO);
		
		forestArea.RendererEnabled = false;
		
		int index = randomiser.GetRandomIntFromRange(0, poolLightColours.Count - 1);
		Color colour = poolLightColours[index];
		CreateLightForArea(poolGO, x, z, (float)areaWidth, (float)areaHeight, colour, 8);
	}
	
	void CreateLightForArea (GameObject parent, int x, int z, float w, float d, Color colour, float intensity) {
		GameObject light = new GameObject("Area Light");
		Light lightComp = light.AddComponent<Light>();
		lightComp.color = colour;
		lightComp.intensity = intensity;
		float y = Mathf.Max(w, d) / 2;
		lightComp.range = y * 2;
		light.transform.parent = parent.transform;
		light.transform.position = new Vector3(x + w / 2, y, z + d / 2);
	}
	
	// Creates the trees, bushes, rocks etc. for the given forestArea
	void CreateForestAreaObjects (ForestArea forestArea, int x, int z, int areaWidth, int areaHeight) {
		forestArea.Ground = CreateGround (x - 0.5f, z - 0.5f, areaWidth, areaHeight, map.locations, groundMaterialMap.getMaterial(randomiser));
		forestArea.Ground.AddComponent(typeof(MeshCollider));	
		forestArea.AddForestObjects(CreateForestObjects ("trees", forestArea.rect, 4f, 0.5f, map.locations, 1.1f, randomiser, treeMaterialMap));
		forestArea.AddForestObjects(CreateForestObjects ("bushes", forestArea.rect, 0.25f, 0.9f, map.locations, 0.4f, randomiser, bushMaterialMap));
		forestArea.AddForestObjects(CreateForestObjects ("rocks", forestArea.rect, 0.2f, 1.5f, map.locations, 0.2f, randomiser, rockMaterialMap));
	}

	// Is the given x, z value inside the existing forestAreas
	// Can be used to switch on the renderer of the forestArea if it is disabled
	bool InForestArea (List<GameObject> forestAreas, int x, int z, bool enableRendererIfDisabled)
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
				//GameObject dividedQuad = MeshUtils.CombineQuads("groundSection", MeshUtils.SubdivideQuad("ground", quad, material, false), material);
				quadPos.x = x + qx + 0.5f;
				quadPos.z = z + qz + 0.5f;
				quad.transform.localPosition = quadPos;
				
				quads[j++] = quad;
			}
		}
		
		GameObject ground = MeshUtils.CombineQuads("ground_" + x + "_" + z, quads, material);
		MeshRenderer mr = ground.GetComponent<MeshRenderer>() as MeshRenderer;
		mr.receiveShadows = true;
		return ground;
	}
	
	// Create a ground object for a forestArea, by creating a quad for each pixel in the map
	// The y position of each corner in the quad is taken from the pregenerated locations map
	// Once these are created, they are merged into a single object
	public GameObject CreateWater (float x, float z, float width, float depth, MapLocation[,] locations, Material material) {
		Vector3[] vertices = GetVerticesForFlatQuad();
		Vector3 quadPos = Vector3.zero;
		quadPos.y = 0.5f;
		GameObject[] quads = new GameObject[Mathf.RoundToInt(depth * width)];
		int j = 0;
		float lowestY = int.MaxValue;
		for (int qz = 0; qz < depth; qz++) {
			for (int qx = 0; qx < width; qx++) {
				
				int hx = Mathf.RoundToInt(x + 0.5f) + qx;
				int hz = Mathf.RoundToInt(z + 0.5f) + qz;
				MapLocation location = locations[hx, hz];
				for (int i = 0; i < 4; i++) {
					if (lowestY > vertices[i].y) lowestY = location.corners[i];
				}
			}
		}
		
		for (int qz = 0; qz < depth; qz++) {
			for (int qx = 0; qx < width; qx++) {
				GameObject quad = MeshUtils.CreatePlane("ground", vertices, material, false);
				//GameObject dividedQuad = MeshUtils.CombineQuads("groundSection", MeshUtils.SubdivideQuad("ground", quad, material, false), material);
				quadPos.x = x + qx + 0.5f;
				quadPos.y = lowestY - 0.01f;
				quadPos.z = z + qz + 0.5f;
				quad.transform.localPosition = quadPos;
				
				quads[j++] = quad;
			}
		}
		
		return MeshUtils.CombineQuads("ground_" + x + "_" + z, quads, material);
	}
	
	// Creates forest objects inside the given rectangle
	public GameObject CreateForestObjects (string name, Rect rect, float scale, float density, MapLocation[,] locations, float minimumDistanceBetween, SeededRandomiser randomiser, ColourMaterialMap materialMap) {
		
		Vector3 holderPos = Vector3.zero;
		holderPos.x = rect.x;
		holderPos.y = 0f;
		holderPos.z = rect.y;
		int totalQuads = Mathf.RoundToInt(rect.width * rect.height * density);
		
		Material material = materialMap.getMaterial(randomiser);
		List<GameObject> objects = new List<GameObject>();
		int timeoutMax = 100;
		int timeout = timeoutMax;
		int objectsMade = 0;
		minimumDistanceBetween *= minimumDistanceBetween;
		while (objectsMade < totalQuads && timeout > 0) {
			float xPosition = randomiser.GetRandomFromRange(0.5f, rect.xMax - rect.xMin - 1f);
			float zPosition = randomiser.GetRandomFromRange(0.5f, rect.yMax - rect.yMin - 1f);
			Vector3 pos = new Vector3(xPosition, 0, zPosition);
			pos.y = (scale / 2) + GetYForPosition(new Vector3(holderPos.x + xPosition, 0, holderPos.z + zPosition)) - 0.01f;
			if (IsValidPosition(pos, objects, objectsMade, minimumDistanceBetween)) {	
				CreateDoubleSidedForestObjectQuad(material, scale, pos, objects);
				timeout = timeoutMax;
				objectsMade++;
			} else {
				timeout--;
			}
		}
		
		GameObject go = MeshUtils.CombineQuads(name, objects.ToArray(), material);
		go.name = name;
		go.transform.localPosition = holderPos;
		
		return go;
	}
	
	// Creates 4 quads, each rotated by 90º from 45º
	// Each quad is assigned a random material from the given MaterialMap
	void CreateDoubleSidedForestObjectQuad (Material material, float scale, Vector3 pos, List<GameObject> objects) {
		for (int rotation = 45; rotation < 360; rotation+=90) {
			GameObject plane = CreateForestObjectQuad(material);
			//plane.transform.parent = parent.transform;
			plane.transform.localScale = new Vector3(scale, scale, scale);
			plane.transform.localPosition = pos;
			plane.transform.localEulerAngles = new Vector3(0, rotation, 0);
			objects.Add(plane);
		}
	}
	
	GameObject CreateBlobShadowCaster (GameObject parent, Vector3 pos) {
		GameObject blob = Instantiate(blobShadowPrefab) as GameObject;
		blob.transform.parent = parent.transform;
		Vector3 tin = new Vector3(0, 0.25f, 0);
		blob.transform.localPosition = pos + tin;
		blob.transform.localEulerAngles = new Vector3(90, 0, 0);
		
		return blob;
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
	bool IsValidPosition (Vector3 position, List<GameObject> existingObjects, int max, float minDist) {
		for (int i = 0; i < max; i++) {
			GameObject existing = existingObjects[i];
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
		if (map == null) return null; 
		return rotated ? map.GetZLocation (position, move) : map.GetXLocation (position, move);
	}
	
	// Given a position, returns the y position at that map location
	public float GetYForPosition (Vector3 position) {
		if (map == null) return 0f;
		return map.GetPositionHeight(position.x, position.z);
	}
}
