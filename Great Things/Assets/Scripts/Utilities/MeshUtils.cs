using UnityEngine;
using System.Collections;

public class MeshUtils {

	public static GameObject CreatePlane(Vector3[] vertices, Material mat, bool reverse) {
	
		// 0:bottom-left, 1:bottom-right, 2:top-left, 3:top-right
		Mesh m = new Mesh();
		m.name = "Scripted_Plane_New_Mesh";
		m.vertices = vertices;
		m.uv = new Vector2[]{new Vector2 (0, 0), new Vector2 (1, 0), new Vector2(1, 1), new Vector2 (0, 1)};
		if (reverse) {
			m.triangles = new int[]
			{0, 1, 2,
				1, 3, 2};
		} else {
		m.triangles = new int[]
			{1, 0, 2,
			1, 2, 3};
		}
		m.RecalculateNormals();
		GameObject plane = new GameObject("New_Plane_From_Script");
		MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
		MeshRenderer meshRenderer = (MeshRenderer)plane.AddComponent(typeof(MeshRenderer));
		meshRenderer.material = mat;
		meshFilter.mesh = m;
		meshRenderer.castShadows = false;
		meshRenderer.receiveShadows = false;
		return plane;
	}
}
