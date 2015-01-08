using UnityEngine;
using System.Collections;

public class CubeBranch {

	public GameObject pivot;
	public GameObject branch;
	public GameObject leafPivot;
	public GameObject leaves;
	public Vector3 branchScale;
	
	public CubeBranch (Material barkMaterial, Material leavesMaterial, Vector3 scale, SeededRandomiser randomiser, float lengthVariance) {
		pivot = new GameObject();
		pivot.name = "pivot";
		pivot.transform.localScale = scale;
		
		float branchRandomThickness = (randomiser.GetRandom() - 0.5f) * 0.05f;
		float branchRandomHeight = randomiser.GetRandom() * lengthVariance;
		branchScale = new Vector3(0.25f + branchRandomThickness,
		                                  1.3f + branchRandomHeight,
		                                  0.25f + branchRandomThickness); 
		
		CreateBranch(scale, barkMaterial);
		CreateCubeLeaves(scale, leavesMaterial, randomiser);
	}
	
	void CreateBranch (Vector3 scale, Material barkMaterial) {
		
		branch = GameObject.CreatePrimitive(PrimitiveType.Cube);
		branch.name = "branch";
		branch.transform.parent = pivot.transform;
		
		branch.transform.localScale = branchScale;
		branch.transform.localPosition = new Vector3(0, branchScale.y / 2.0f, 0);
		
		// texture
		Vector2 texScale = new Vector2(branchScale.x * 2 * scale.x, branchScale.y * 2 * scale.x);
		MeshRenderer renderer = branch.GetComponent<MeshRenderer>();
		renderer.material = barkMaterial;
		renderer.material.SetTextureScale("_MainTex", texScale);
		renderer.material.SetTextureScale("_BumpMap", texScale);
	}
	
	void CreateCubeLeaves (Vector3 scale, Material leavesMaterial, SeededRandomiser randomiser) {
	
		float thickness = randomiser.GetRandom() + 0.25f;
		//float tallness = randomiser.GetRandom() + 1f;
		float leavesAxisLength = 1.8f + ((randomiser.GetRandom() - 0.25f) * 0.25f);
		
		leafPivot = new GameObject();
		leafPivot.transform.parent = pivot.transform;
		leafPivot.transform.localPosition = new Vector3(0, branchScale.y, 0);
		leafPivot.transform.localScale = new Vector3(thickness, thickness, thickness);
		
		leaves = GameObject.CreatePrimitive(PrimitiveType.Cube);
		leaves.name = "leaves";
		leaves.transform.parent = leafPivot.transform;
		Vector3 leavesScale = new Vector3(leavesAxisLength, leavesAxisLength, leavesAxisLength);
		leaves.transform.localScale = leavesScale;
		leaves.transform.localPosition = new Vector3(0, leavesAxisLength / 2, 0);
		leaves.transform.localEulerAngles = new Vector3(-36.0f, 0, 45.0f);
		leaves.AddComponent("CubeTexturer");
		
		// texture
		//Vector2 texScale = new Vector2(leavesAxisLength * scale.x, leavesAxisLength * scale.x);
		MeshRenderer renderer = leaves.GetComponent<MeshRenderer>();
		renderer.material = leavesMaterial;
		//renderer.material.SetTextureScale("_MainTex", texScale);
		//renderer.material.SetTextureScale("_BumpMap", texScale);
	}
}
