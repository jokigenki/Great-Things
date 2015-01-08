using UnityEngine;
using System.Collections;

public class CubeTree {

	public GameObject root;
	public CubeBranch trunk;
	public CubeBranch mediumBranch;
	public CubeBranch smallBranch;
	
	public CubeTree (string name, Material barkMaterial, Material leavesMaterial, SeededRandomiser randomiser, float heightVariance) {
		root = new GameObject();
		root.name = name;
		CreateTrunk (barkMaterial, leavesMaterial, randomiser, heightVariance);
		//CreateMediumBranch(barkMaterial, leavesMaterial, randomiser);
		//CreateSmallBranch(barkMaterial, leavesMaterial, randomiser);
	}
	
	void CreateTrunk (Material barkMaterial, Material leavesMaterial, SeededRandomiser randomiser, float heightVariance) {
		trunk = new CubeBranch(barkMaterial, leavesMaterial, new Vector3(1, 1, 1), randomiser, heightVariance);
		trunk.pivot.transform.parent = root.transform;
	}
	
	void CreateMediumBranch (Material barkMaterial, Material leavesMaterial, SeededRandomiser randomiser) {
		float branchPosition = trunk.branchScale.y / 1.3f * randomiser.GetRandomFromRange(0.75f, 0.95f);
		float branchRotation = randomiser.GetRandomFromRange(-70, 70);
		mediumBranch = new CubeBranch(barkMaterial, leavesMaterial, new Vector3(0.4f, 0.4f, 0.4f), randomiser, 1);
		mediumBranch.pivot.transform.parent = root.transform;
		mediumBranch.pivot.transform.localPosition = new Vector3(0, branchPosition, -0.08f);
		mediumBranch.pivot.transform.localEulerAngles = new Vector3(-50.0f, branchRotation, 0);
	}
	
	void CreateSmallBranch (Material barkMaterial, Material leavesMaterial, SeededRandomiser randomiser) {
		float branchPosition = trunk.branchScale.y / 1.3f * randomiser.GetRandomFromRange(0.55f, 0.75f);
		float branchRotation = randomiser.GetRandomFromRange(110, 250);
		smallBranch = new CubeBranch(barkMaterial, leavesMaterial, new Vector3(0.3f, 0.3f, 0.3f), randomiser, 1);
		smallBranch.pivot.transform.parent = root.transform;
		smallBranch.pivot.transform.localPosition = new Vector3(0, branchPosition, 0.1f);
		smallBranch.pivot.transform.localEulerAngles = new Vector3(-42.0f, branchRotation, 0);
	}
}
