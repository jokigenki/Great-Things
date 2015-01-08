using UnityEngine;
using System.Collections;

public class BranchMesh
{
	public string name;
	public int generation;
	public MeshData data; // holds the mesh data for this mesh
	public int numberOfBranches; // the number of branches that will be generated at each joint
	public float bendAmount; // the amount the mesh bends between the bottom and top vertices
	public float bendDirection; // the direction the branch bends in
	public float branchBendAmount; // the amount child branches will start their bend
	public float lengthToThicknessRatio; // when the mesh grows by 1 in length, how much it will grow in thickness
	public float currentLengthToThicknessRatio; // this value will change as the tree grows, depending on the leanness value
	public float buddingRatio; // once the tree reaches this lengthToThicknessRatio, it will branch
	public float leanness; // higher values will produce taller, thinner trees
	public float taper; // the difference between the bottom and top thickness
	public BranchMesh[] childBranches; // any branches that are growing from this branch
	public float length;
	public Color colour1;
	public Color colour2;
	public Color colour3;
	public Color colour4;
	public SeededRandomiser randomiser;
	
	public BranchMesh (string name, int seed, float length, float baseSize, float taper, float leanness, float buddingRatio, int numberOfBranches, float branchBendAmount)
	{
		this.randomiser = new SeededRandomiser(seed);
		this.generation = 0;
		this.name = name;
		this.length = length;
		this.lengthToThicknessRatio = (1 / length) * baseSize;
		this.currentLengthToThicknessRatio = this.lengthToThicknessRatio;
		this.leanness = leanness;
		this.buddingRatio = buddingRatio;
		this.bendAmount = 0;
		this.bendDirection = 0;
		this.branchBendAmount = branchBendAmount;
		this.numberOfBranches = numberOfBranches;
		
		CreateInitialVertices();
		
		float hw = baseSize / 2;
		SetVertex(data.vertices, 0, -hw, 0, -hw);
		SetVertex(data.vertices, 1, hw, 0, -hw);
		SetVertex(data.vertices, 2, hw, 0, hw);
		SetVertex(data.vertices, 3, -hw, 0, hw);
		colour1 = Color.red;
		colour2 = Color.green;
		colour3 = Color.blue;
		colour4 = Color.magenta;
		
		CreateMesh (length, taper);
		
	}
	
	public BranchMesh (string name, float length, BranchMesh parent, float taper, float bendAmount, float bendDirection)
	{
		this.randomiser = parent.randomiser;
		this.generation = parent.generation + 1;
		this.name = name;
		this.length = length;
		this.leanness = parent.leanness;
		this.lengthToThicknessRatio = parent.lengthToThicknessRatio;
		this.currentLengthToThicknessRatio = this.lengthToThicknessRatio;
		this.buddingRatio = parent.buddingRatio;
		this.numberOfBranches = parent.numberOfBranches;
		this.branchBendAmount = parent.branchBendAmount + randomiser.GetRandomFromRange(0, 10);
		
		this.bendAmount = bendAmount;
		this.bendDirection = bendDirection;
		CreateInitialVertices();
		CopyVertices(parent);
		
		colour1 = Color.red;//new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
		colour2 = Color.green;//new Color(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f));
		colour3 = Color.blue;
		colour4 = Color.magenta;
		
		CreateMesh (length, taper);
	}
	
	public void SetVertex (Vector3[] vertices, int index, float x, float y, float z) {
		vertices[index].x = x;
		vertices[index].y = y;
		vertices[index].z = z;
	}
	
	public void CopyVertices (BranchMesh branch) {
		data.AssignToBaseVertices(branch.data.GetEndVertices());
	}
	
	void CreateInitialVertices () {
		this.data = new MeshData();
		data.vertices = new Vector3[8];
		for (int i = 0; i < 8; i++) {
			data.vertices[i] = Vector3.zero;
		}
	}
	
	public void Grow (float amount) {
		length += amount * (1 - currentLengthToThicknessRatio);
		data.AssignToBaseVertices(GrowSet(amount * lengthToThicknessRatio, data.GetBaseVertices()));
		CreateMesh(length, taper);
		
		if (childBranches != null) {
			foreach (BranchMesh branch in childBranches) { 
				branch.GrowAsChild(this, amount);
			}
		} else if (currentLengthToThicknessRatio > buddingRatio) {
			childBranches = new BranchMesh[numberOfBranches];
			for (int i = 0; i < numberOfBranches; i++) {
				float bendDirection = (360 / numberOfBranches * i) + (generation * 90) +  (randomiser.GetRandomFromRange(-60, 60));
				BranchMesh childBranch = new BranchMesh("branch_" + generation + "_" + i, 0.01f, this, this.taper, branchBendAmount, bendDirection);
				childBranches[i] = childBranch;
			}
		}
		
		if (currentLengthToThicknessRatio < 0.98) {
			currentLengthToThicknessRatio += amount * taper / leanness;
		}
	}
	
	public void GrowAsChild (BranchMesh parent, float amount) {
		length += amount * (1 - currentLengthToThicknessRatio);
		CopyVertices(parent);
		CreateMesh(length, taper);
		bendAmount += 0.1f / generation;
		
		if (childBranches != null) {
			foreach (BranchMesh branch in childBranches) { 
				branch.GrowAsChild(this, amount);
			}
		} else if (currentLengthToThicknessRatio > buddingRatio) {
			childBranches = new BranchMesh[numberOfBranches];
			for (int i = 0; i < numberOfBranches; i++) {
				float bendDirection = (360 / numberOfBranches * i) + (generation * 90) +  (randomiser.GetRandomFromRange(-20, 20));
				BranchMesh childBranch = new BranchMesh("branch_" + generation + "_" + i, 0.01f, this, this.taper, branchBendAmount, bendDirection);
				childBranches[i] = childBranch;
			}
		}
		
		if (currentLengthToThicknessRatio < 0.98) {
			currentLengthToThicknessRatio += amount * taper / leanness;
		}
	}
	
	public Vector3[] GrowSet (float amount, Vector3[] vertices) {
		Vector3 pivot = GetPivot(vertices);
		
		for (int i = 0; i < 4; i++) {
			Vector3 offset = (vertices[i] - pivot) * amount;
			vertices[i] += offset;
		}
		
		return vertices;
	}
	
	void CreateMesh (float length, float taper)
	{	
		this.taper = taper;
		
		// find pivot of bottom face, and rotate to match angle
		Vector3 pivot = GetPivot (data.GetBaseVertices());
		Vector3 normal = data.GetBaseNormal();
		Vector3 branchEnd = pivot + normal * length;
		
		Vector3 rotator = new Vector3(0, bendDirection, bendAmount);
		branchEnd = RotatePointAroundPivot(branchEnd, pivot, rotator);
		
		Vector3[] rotated = SortPoints(RotatePointsAroundPivot(data.GetBaseVertices(), pivot, rotator));
		
		normal = GetNormal(rotated[0], rotated[1], rotated[2]) * length;
		
		data.AssignToEndVertices(TaperPoints(branchEnd, rotated, normal, taper));
		
		Vector3[] baseV = data.GetBaseVertices();
		Vector3[] endV = data.GetEndVertices();
		Debug.DrawLine(pivot, branchEnd, Color.white);
		Debug.DrawLine(baseV[0], endV[0], colour1);
		Debug.DrawLine(baseV[1], endV[1], colour2);
		Debug.DrawLine(baseV[2], endV[2], colour3);
		Debug.DrawLine(baseV[3], endV[3], colour4);
		
		data.uvs = new Vector2[] {
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (0, 0),
			new Vector2 (1, 0),
			new Vector2 (0, 1),
			new Vector2 (1, 1),
			new Vector2 (0, 1),
			new Vector2 (1, 1)
		};
		data.triangles = new int[]{1, 0, 4, 1, 4, 5,
			2, 1, 5, 2, 5, 6,
			3, 2, 6, 3, 6, 7,
			0, 3, 7, 0, 7, 4,
			5, 4, 7, 5, 7, 6};
	}
	
	// finds the point which has the combined lowest x and z values,
	// then arranges the points so they follow from this point
	Vector3[] SortPoints (Vector3[] points) {
		float lowestPointValue = int.MaxValue;
		
		int lowestIndex = -1;
		for (int i = 0; i < points.Length; i++) {
			Vector3 point = points[i];
			float pointValue = point.x + point.z;
			if (pointValue < lowestPointValue) {
				lowestIndex = i;
				lowestPointValue = pointValue;
			}
		}
		
		if (lowestIndex < 1) return points;
		
		int j = 0;
		Vector3[] sortedPoints = new Vector3[4];
		for (int i = lowestIndex; i < points.Length; i++) {
			sortedPoints[j++] = points[i];
		}
		
		for (int i = 0; i < lowestIndex; i++) {
			sortedPoints[j++] = points[i];
		}
		
		return sortedPoints;
	}
	
	Vector3[] TaperPoints (Vector3 taperCentre, Vector3[] pointsToTaper, Vector3 offset, float taper) {
		Vector3[] tapered = new Vector3[pointsToTaper.Length];
		int i = 0;
		foreach (Vector3 point in pointsToTaper) {
			tapered[i++] = TaperPoint(taperCentre, point + offset, taper);
		}
		
		return tapered;
	}
	
	Vector3 TaperPoint (Vector3 taperCentre, Vector3 pointToTaper, float taper) {
		Vector3 offset = pointToTaper - taperCentre;
		return taperCentre + (offset / taper); 
	}
	
	Vector3[] RotatePointsAroundPivot (Vector3[] vertices, Vector3 pivot, Vector3 angles) {
		Vector3[] rotated = new Vector3[vertices.Length];
		for (int i = 0; i < 4; i++) {
			rotated[i] = RotatePointAroundPivot(vertices[i], pivot, angles);
		}
		
		return rotated;
	}
	
	void RotatePointAroundPivot (Vector3[] vertices, int index, Vector3 pivot, Vector3 angles) {
		Vector3 point = vertices[index];
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler (angles) * dir;
		vertices[index] = dir + pivot;
	}
	
	Vector3 RotatePointAroundPivot (Vector3 point, Vector3 pivot, Vector3 angles)
	{
		Vector3 dir = point - pivot;
		dir = Quaternion.Euler (angles) * dir;
		return dir + pivot;
	}
	
		Vector3 GetNormal (Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 v1 = p3 - p1;
		Vector3 v2 = p2 - p1;
		Vector3 normal = Vector3.Cross(v1, v2);
		normal /= normal.magnitude;
		return normal;
	}
	
	Vector3 GetPivot (Vector3[] vertices)
	{
		Vector3 pivot = Vector3.zero;
		for (int i = 0; i < 4; i++) 
		{
			pivot.x += vertices[i].x;
			pivot.y += vertices[i].y;
			pivot.z += vertices[i].z;
		}
		pivot /= 4;
		
		return pivot; 
	}
	
	public MeshData GetCombinedMesh (MeshData parentData, int triangleOffset) {

		Vector3[] pCombined = parentData.vertices;
		Vector3[] tCombined = data.vertices;
		Vector3[] newVertices = new Vector3[pCombined.Length + tCombined.Length];
		
		pCombined.CopyTo(newVertices, 0);
		tCombined.CopyTo(newVertices, pCombined.Length);
		
		Vector2[] newUVs = new Vector2[parentData.uvs.Length + data.uvs.Length];
		parentData.uvs.CopyTo(newUVs, 0);
		data.uvs.CopyTo(newUVs, parentData.uvs.Length);
		
		int k = parentData.triangles.Length;
		int[] newTris = new int[parentData.triangles.Length + data.triangles.Length];
		parentData.triangles.CopyTo(newTris, 0);
		foreach (int triangle in data.triangles) {
			newTris[k++] = triangle + triangleOffset;
		}
		
		parentData.vertices = newVertices;
		parentData.uvs = newUVs;
		parentData.triangles = newTris;
		
		if (childBranches == null) return parentData;
		
		foreach (BranchMesh branch in childBranches) {
			parentData = branch.GetCombinedMesh(parentData, parentData.vertices.Length);	
		}
		
		return parentData;
	}
}
