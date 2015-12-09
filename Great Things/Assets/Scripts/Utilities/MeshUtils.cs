using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshUtils {

	public static GameObject CreatePlane(string name, Vector3[] vertices, Material mat, bool reverse) {
	
		// 0:bottom-left, 1:bottom-right, 2:top-left, 3:top-right
		// 2-3  F  C  B-C    R  C  C-B
		// | |  O  |\  \|    E  |\  \|    
		// 0-1  R  B-A  A    V  A-B  A
		Mesh m = new Mesh();
		m.name = "Scripted_Plane_New_Mesh";
		m.vertices = vertices;
		m.uv = new Vector2[]{new Vector2 (0, 0), new Vector2 (1, 0), new Vector2(1, 1), new Vector2 (0, 1)};
		if (reverse) {
			m.triangles = new int[] {0, 1, 2, 1, 3, 2}; 
		} else {
			m.triangles = new int[] {1, 0, 2, 1, 2, 3};
		}
		
		m.RecalculateNormals();
		GameObject plane = new GameObject(name);
		MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
		meshFilter.mesh = m;
		plane.AddComponent(typeof(MeshCollider));
		MeshRenderer meshRenderer = (MeshRenderer)plane.AddComponent(typeof(MeshRenderer));
		if (mat != null) meshRenderer.material = mat;
		meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		meshRenderer.receiveShadows = false;
		return plane;
	}
	
	public static GameObject[] SubdivideQuad(string name, GameObject quad, Material mat, bool reverse) {
		// we have 4 vertices, and we need to make some more
		// 2-3   2 8 3   c i d
		// | |   5 6 7   f g h
		// 0-1   0 4 1   a e b 
		
		MeshFilter meshFilter = quad.GetComponent<MeshFilter>();
		Vector3[] vertices = meshFilter.mesh.vertices;
		Vector3 a = vertices[0];
		Vector3 b = vertices[1];
		Vector3 c = vertices[2];
		Vector3 d = vertices[3];
		Vector3 e = ((b - a) * 0.5f) + a;
		Vector3 f = ((c - a) * 0.5f) + a;
		Vector3 g = ((d - a) * 0.5f) + a;
		Vector3 h = ((d - b) * 0.5f) + b;
		Vector3 i = ((d - c) * 0.5f) + c;
		GameObject p1 = CreatePlane(name + "_1", new Vector3[]{a, e, f, g}, mat, reverse);
		GameObject p2 = CreatePlane(name + "_2", new Vector3[]{e, b, g, h}, mat, reverse);
		GameObject p3 = CreatePlane(name + "_3", new Vector3[]{f, g, c, i}, mat, reverse);
		GameObject p4 = CreatePlane(name + "_4", new Vector3[]{g, h, i, d}, mat, reverse);
		
		GameObject.Destroy(quad);
		
		return new GameObject[]{p1, p2, p3, p4};
	}
	
	// Subdivides the given quad the number of times given in divisions.
	// Each subdivision will return 4x more quads than previous, i.e. 1, 4, 16, 64, 256 etc.
	// Once the subdivision is complete, the object is merged into a single object
	public static GameObject[] SubdivideQuad(string name, GameObject quadToDivide, int divisions, Material mat, bool reverse) {
		
		List<GameObject> quadsToDivide = new List<GameObject>();
		quadsToDivide.Add(quadToDivide);
		for (int i = 0; i < divisions; i++) {
			List<GameObject> newQuads = new List<GameObject>();
			foreach (GameObject quad in quadsToDivide) {
				GameObject[] dividedQuads = SubdivideQuad(name + "_" + i, quad, mat, reverse);
				foreach (GameObject dividedQuad in dividedQuads) {
					newQuads.Add(dividedQuad);
				}
			}
			quadsToDivide = newQuads;
		}
		
		return quadsToDivide.ToArray();
	}
	
	// Takes an array of quads and turns them into a single mesh
	public static GameObject CombineQuads (string name, GameObject[] gObjs, Material material, bool destroyAfterCombining = true) {
		GameObject combined = new GameObject();
		combined.name = name;
		combined.AddComponent(typeof(MeshFilter));
		
		CombineInstance[] combine = new CombineInstance[gObjs.Length];
		int k = 0;
		while (k < gObjs.Length) {
			GameObject gObj = gObjs[k];
			gObj.transform.parent = combined.transform;
			MeshFilter quadMeshFilter = gObj.GetComponent<MeshFilter>();
			combine[k].mesh = quadMeshFilter.sharedMesh;
			combine[k].transform = quadMeshFilter.transform.localToWorldMatrix;
			if (destroyAfterCombining) GameObject.Destroy(gObj);
			k++;
		}
		
		combined.transform.gameObject.SetActive(true);
		
		Mesh mesh = new Mesh();
		mesh.CombineMeshes(combine);
		mesh.Optimize();
		mesh.RecalculateNormals();
		
		MeshFilter meshFilter = combined.transform.GetComponent<MeshFilter>();
		meshFilter.mesh = mesh;
		
		MeshRenderer mr = (MeshRenderer)combined.AddComponent(typeof(MeshRenderer));
		mr.material = material;
		mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		mr.receiveShadows = false;
		
		return combined;
	}
	
	public static List<int> GetMatchedVertices (Vector3 matcher, Vector3[] vertices) {
		List<int> matched = new List<int>();
		
		int i = 0;
		foreach (Vector3 vertex in vertices) {
			if (vertex.x == matcher.x &&
			    vertex.y == matcher.y &&
			    vertex.z == matcher.z) matched.Add(i);
			i++;
		}
		
		return matched;
	}
	
	public static void JitterMeshOnY (GameObject gObj, int min, int max, float scalar, float clampMin, float clampMax) {
		Mesh mesh = gObj.GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		int i = 0;
		while (i < vertices.Length) {
			List<int> matched = MeshUtils.GetMatchedVertices(vertices[i], vertices);
			float value = Random.Range(min, max) * scalar;
			for (int j = 0; j < matched.Count; j++) {
				int index = matched[j];
				float current = vertices[index].y;
				float newValue = current + value;
				if (newValue < clampMin) {
					newValue = clampMin;
				} else if (newValue > clampMax) {
					newValue = clampMax;
				}
				vertices[index].y = newValue;
			}
			i++;
		}
		mesh.vertices = vertices;
	}
	
	public static void AdjustMeshAlongNormals (GameObject gObj, float value) {
		Mesh mesh = gObj.GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		int i = 0;
		while (i < vertices.Length) {
			List<int> matched = MeshUtils.GetMatchedVertices(vertices[i], vertices);
			for (int j = 0; j < matched.Count; j++) {
				vertices[matched[j]] += normals[i] * value;
			}
			i++;
		}
		mesh.vertices = vertices;
	}
}
