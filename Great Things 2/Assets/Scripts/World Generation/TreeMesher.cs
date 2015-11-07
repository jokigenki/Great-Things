using UnityEngine;
using System.Collections;

public class TreeMesher : MonoBehaviour {

	public Material mat;
	BranchMesh trunk;
	
	// Use this for initialization
	void Start () {
		Grow ();
	}
	
	// Update is called once per frame
	void Update () {
		float growAmount = 0.01f;
		trunk.Grow(growAmount);
		
		UpdateMesh();
	}
	
	public void Grow () {
		
		trunk = new BranchMesh("one", 1232187, 0.2f, 0.05f, 1.4f, 2.5f, 0.6f, 2, 10f);
		
		Mesh m = GetCombinedMesh(trunk);
		
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));
		MeshRenderer meshRenderer = (MeshRenderer)gameObject.AddComponent(typeof(MeshRenderer));
		meshRenderer.material = mat;
		meshRenderer.receiveShadows = false;
		
		meshFilter.mesh = m;
	}
	
	public void UpdateMesh () {
		MeshFilter meshFilter = (MeshFilter)gameObject.GetComponent(typeof(MeshFilter));
		Mesh m = GetCombinedMesh(trunk);
		
		meshFilter.mesh = m;
	}
	
	public Mesh GetCombinedMesh (BranchMesh branch) {
		MeshData combined = branch.GetCombinedMesh(new MeshData(), 0);
		
		Mesh m = new Mesh();
		m.name = "TreeMesh";
		m.vertices = combined.vertices;
		m.uv = combined.uvs; 
		m.triangles = combined.triangles;
		m.RecalculateNormals();
		
		return m;
	}
}
