using UnityEngine;
using System.Collections;

public class MeshData {
	
	public Vector3[] vertices; // holds the vertices for the bottom face of this mesh
	public int[] triangles; // holds the triangles for this mesh
	public Vector2[] uvs; // holds the uvs for this mesh
	
	public MeshData () {
		vertices = new Vector3[]{};
		triangles = new int[]{};
		uvs = new Vector2[]{};
	}
	
	public Vector3[] GetBaseVertices () {
		return new Vector3[]{vertices[0], vertices[1], vertices[2], vertices[3]};
	}
	
	public void AssignToBaseVertices (Vector3[] vertices) {
		this.vertices[0] = vertices[0];
		this.vertices[1] = vertices[1];
		this.vertices[2] = vertices[2];
		this.vertices[3] = vertices[3];
	}
	
	public Vector3[] GetEndVertices () {
		return new Vector3[]{vertices[4], vertices[5], vertices[6], vertices[7]};
	}
	
	public void AssignToEndVertices (Vector3[] vertices) {
		this.vertices[4] = vertices[0];
		this.vertices[5] = vertices[1];
		this.vertices[6] = vertices[2];
		this.vertices[7] = vertices[3];
	}
	
	public Vector3 GetBaseNormal () {
		return GetNormal(GetBaseVertices());
	}
	
	public Vector3 GetEndNormal () {
		return GetNormal(GetEndVertices());
	}
	
	public Vector3 GetNormal(Vector3[] vertices) {
			Vector3 v1 = vertices[2] - vertices[0];
			Vector3 v2 = vertices[1] - vertices[0];
		Vector3 normal = Vector3.Cross(v1, v2);
		normal /= normal.magnitude;
		return normal;
	}
}
