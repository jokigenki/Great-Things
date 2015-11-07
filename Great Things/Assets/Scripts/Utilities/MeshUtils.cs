using UnityEngine;
using System.Collections;

public class MeshUtils {

	public static GameObject CreatePlane(string name, Vector3[] vertices, Material mat, bool reverse) {
	
		// 0:bottom-left, 1:bottom-right, 2:top-left, 3:top-right
		// 0:+x+z, -x+z, +x-z, -x-z 
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
}
