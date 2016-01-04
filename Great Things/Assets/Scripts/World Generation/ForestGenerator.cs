using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ForestGenerator : MonoBehaviour
{
	public List<ColourMaterialMap> materialMaps;
	
	public List<Color> poolLightColours;
	public List<Color> forestLightColours;
	
	public GameObject player;
	public Texture2D pathMap;
	public int hilliness = 2;
	public int drawDistance = 10;
	public float pathInsetAmount = 0.05f;
	public bool saveToPrefabOnComplete = false;
	
	Locale forest;
	
	Vector3 pathInset;
	
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
		pathInset = new Vector3(0, pathInsetAmount, 0);
		
		SeededRandomiser randomiser = new SeededRandomiser(12345678);
		Map map = CreateMap(randomiser);
		CreateForestParentObjects(map, randomiser);
		
		StartCoroutine(StartGeneratorQueue());
	}
	
	IEnumerator StartGeneratorQueue () {
		generationQueue.Enqueue(()=>CreatePath(forest.map));
		generationQueue.Enqueue(()=>CreateForestAreas(forest.map));
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
	
	ColourMaterialMap GetMaterialMapForName (string name) {
		foreach (ColourMaterialMap map in materialMaps) {
			if (map.name == name) return map;
		}
		
		return null;
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
	
	void CreateForestParentObjects (Map map, SeededRandomiser randomiser) {
		GameObject forestGO = new GameObject ();
		forestGO.name = "Forest";
		forestGO.transform.parent = transform;
		
		forest = forestGO.AddComponent(typeof(Locale)) as Locale;
		forest.map = map;
		forest.drawDistance = drawDistance;
		forest.SetRandomiser(randomiser);
		forest.PositionPlayer(player);
		
		Vector3 pos = Vector3.zero;
		pos.y = 0.5f;
		forestGO.transform.localPosition = pos;
		
		forest.sublocales = new List<GameObject>();
	}
	
	Map CreateMap (SeededRandomiser randomiser) {
	
		MapTag[] tags = {new MapTag(PATH_PIXEL, "path"), 
			new MapTag(ENTRANCE_PIXEL, "entrance"),
			new MapTag(FOREST_PIXEL, "forest"),
			new MapTag(FOREST_PIXEL_2, "forest"),
			new MapTag(POOL, "pool")};
			
		Map map = new Map(pathMap, new MapTag(BLANK_PIXEL, "empty"), tags, hilliness);
		MapUtils.SetHeightsForMap(map, randomiser);
		
		return map;
	}
	
	// Creates the paths for the map. Each square is a separate object, which it then merged into a single object.
	IEnumerator CreatePath (Map map)
	{
		Vector3[] vertices = GetVerticesForFlatQuad();
		Vector3 position = Vector3.zero;
		
		Material material = GetMaterialMapForName("Path").getMaterial(forest.randomiser);
		Material underMaterial = GetMaterialMapForName("Ground").getMaterial(forest.randomiser);
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
		paths.transform.parent = forest.gameObject.transform;
		paths.transform.position = paths.transform.position - pathInset;
		paths.AddComponent(typeof(MeshCollider));
		GameObjectUtility.SetStaticEditorFlags(paths, StaticEditorFlags.LightmapStatic);
		
		GameObject underPaths = MeshUtils.CombineQuads("underPaths", underQuads.ToArray(), underMaterial, true);
		underPaths.transform.parent = forest.gameObject.transform;
		
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
		MapLocation location = forest.map.GetMapLocationForPosition(position);
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
				if (ShouldCreateAForestArea(pixel, x, z, GetMaterialMapForName("Ground"))) {
					CreateForestArea(pixel, currentPixel, x, z);
					yield return null;
				} else if (ShouldCreateAForestArea(pixel, x, z, GetMaterialMapForName("Water"))) {
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
		
		if (InForestArea (forest.sublocales, x, z, true)) return false;
		foreach (Color color in matMap.colors) {
			if (ColourUtils.Match (pixel, color)) return true;
		}
		
		return false;
	}
	
	IEnumerator CompleteSetup() {
	
		Camera mainCamera = Camera.main;
		SmoothCameraTracker sct = mainCamera.GetComponent<SmoothCameraTracker>();
		sct.locale = forest;
		forest.SetReady(player);
		
		if (saveToPrefabOnComplete) {
			PrefabUtils.GenerateNestedPrefab(forest.gameObject, "Meshes", "Prefabs");
		}
		yield return null;
	}
	
	public void CreateForestArea (Color pixel, Color currentPixel, int x, int z) {
		GameObject forestGO = new GameObject ();
		forestGO.name = "forestArea";
		forestGO.transform.parent = forest.transform;
		Sublocale forestArea = forestGO.AddComponent <Sublocale>() as Sublocale;
		int areaWidth = 0;
		forestArea.type = "forest";
		int areaHeight = 0;
		while (pixel.Equals(currentPixel)) {
			areaHeight++;
			pixel = forest.map.GetPixelForLocation (x, z + areaHeight);
		}
		pixel = forest.map.GetPixelForLocation (x, z);
		while (pixel.Equals(currentPixel)) {
			areaWidth++;
			pixel = forest.map.GetPixelForLocation (x + areaWidth, z);
		}
		
		forestArea.rect = new Rect(x, z, areaWidth, areaHeight);
		CreateForestSublocaleFeatures(forestArea, x, z, areaWidth, areaHeight);
		forest.sublocales.Add (forestGO);
		
		forestArea.RendererEnabled = false;
		
		//int index = forest.randomiser.GetRandomIntFromRange(0, forestLightColours.Count - 1);
		//Color colour = forestLightColours[index];
		//CreateLightForArea(forestGO, x, z, (float)areaWidth, (float)areaHeight, colour, 4);
	}
	
	public void CreatePoolArea (Color pixel, Color currentPixel, int x, int z) {
		GameObject poolGO = new GameObject ();
		poolGO.name = "poolArea";
		poolGO.transform.parent = forest.transform;
		Sublocale forestArea = poolGO.AddComponent <Sublocale>() as Sublocale;
		forestArea.type = "pool";
		int areaWidth = 0;
		int areaHeight = 0;
		while (pixel.Equals(currentPixel)) {
			areaHeight++;
			pixel = forest.map.GetPixelForLocation (x, z + areaHeight);
		}
		pixel = forest.map.GetPixelForLocation (x, z);
		while (pixel.Equals(currentPixel)) {
			areaWidth++;
			pixel = forest.map.GetPixelForLocation (x + areaWidth, z);
		}
	
		forestArea.rect = new Rect(x, z, areaWidth, areaHeight);
		forestArea.Ground = CreateWater (x - 0.5f, z - 0.5f, areaWidth, areaHeight, forest.map, GetMaterialMapForName("Water").getMaterial(forest.randomiser));
		MeshUtils.JitterMeshOnY(forestArea.Ground, -10, 10, 0.01f, -0.1f, 0.1f);
		forest.sublocales.Add (poolGO);
		
		forestArea.RendererEnabled = false;
		
		int index = forest.randomiser.GetRandomIntFromRange(0, poolLightColours.Count - 1);
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
	void CreateForestSublocaleFeatures (Sublocale forestSublocale, int x, int z, int areaWidth, int areaHeight) {
		SeededRandomiser randomiser = forest.randomiser;
		Map map = forest.map;
		forestSublocale.Ground = CreateGround (x - 0.5f, z - 0.5f, areaWidth, areaHeight, map, GetMaterialMapForName("Ground").getMaterial(randomiser));
		forestSublocale.Ground.AddComponent(typeof(MeshCollider));	
		forestSublocale.AddFeatures(CreateForestFeatures ("trees", forestSublocale.rect, 4f, 0.5f, map, 1.1f, randomiser, GetMaterialMapForName("Tree")));
		forestSublocale.AddFeatures(CreateForestFeatures ("bushes", forestSublocale.rect, 0.25f, 0.9f, map, 0.4f, randomiser, GetMaterialMapForName("Bush")));
		forestSublocale.AddFeatures(CreateForestFeatures ("rocks", forestSublocale.rect, 0.2f, 1.5f, map, 0.2f, randomiser, GetMaterialMapForName("Rock")));
	}

	// Is the given x, z value inside the existing forestAreas
	// Can be used to switch on the renderer of the forestArea if it is disabled
	bool InForestArea (List<GameObject> forestAreas, int x, int z, bool enableRendererIfDisabled)
	{
		foreach (GameObject area in forestAreas) {
			Sublocale maker = area.GetComponent<Sublocale> ();
			if (maker.Contains (x, z)) {
				if (enableRendererIfDisabled && !maker.RendererEnabled) maker.RendererEnabled = true;
				return true;
			}
		}
		
		return false;
	}
	
	// Create a ground object for a forestArea, by creating a quad for each pixel in the map
	// The y position of each corner in the quad is taken from the pregenerated locations map
	// Once these are created, they are merged into a single object
	public GameObject CreateGround (float x, float z, float width, float depth, Map map, Material material) {
		Vector3[] vertices = GetVerticesForFlatQuad();
		Vector3 quadPos = Vector3.zero;
		quadPos.y = 0.5f;
		GameObject[] quads = new GameObject[Mathf.RoundToInt(depth * width)];
		int j = 0;
		for (int qz = 0; qz < depth; qz++) {
			for (int qx = 0; qx < width; qx++) {
				
				int hx = Mathf.RoundToInt(x + 0.5f) + qx;
				int hz = Mathf.RoundToInt(z + 0.5f) + qz;
				MapLocation location = map.GetMapLocationForPosition(hx, hz);
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
	public GameObject CreateWater (float x, float z, float width, float depth, Map map, Material material) {
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
				MapLocation location = map.GetMapLocationForPosition(hx, hz);
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
	public SublocaleFeatures CreateForestFeatures (string name, Rect rect, float scale, float density, Map map, float minimumDistanceBetween, SeededRandomiser randomiser, ColourMaterialMap materialMap) {
		
		SublocaleFeatures forestObjects = new SublocaleFeatures();
		
		Vector3 holderPos = Vector3.zero;
		holderPos.x = rect.x;
		holderPos.y = 0f;
		holderPos.z = rect.y;
		int totalQuads = Mathf.RoundToInt(rect.width * rect.height * density);
		
		GameObject shadowCasters = new GameObject();
		shadowCasters.transform.localPosition = holderPos;
		shadowCasters.name = name + "_shadowCasters";
		
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
			pos.y = (scale / 2) + forest.GetYForPosition(new Vector3(holderPos.x + xPosition, 0, holderPos.z + zPosition)) - 0.01f;
			if (IsValidPosition(pos, objects, objectsMade, minimumDistanceBetween)) {	
				CreateDoubleSidedForestObjectQuad(material, scale, pos, objects);
				timeout = timeoutMax;
				objectsMade++;
			} else {
				timeout--;
			}
			/*
			GameObject shadowCaster = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			shadowCaster.name = "shadowCaster_" + objectsMade;
			shadowCaster.transform.localPosition = pos;
			shadowCaster.transform.localScale = new Vector3(scale / 2, scale / 2, scale / 2);
			shadowCaster.transform.parent = shadowCasters.transform;
			MeshRenderer mr = shadowCaster.GetComponent<MeshRenderer>();
			mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
			*/
		}
		
		GameObject go = MeshUtils.CombineQuads(name, objects.ToArray(), material);
		go.name = name;
		go.transform.localPosition = holderPos;
		
		forestObjects.features = go;
		forestObjects.shadowCasters = shadowCasters;
		return forestObjects;
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
}
