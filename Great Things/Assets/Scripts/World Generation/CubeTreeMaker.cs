using UnityEngine;
using System.Collections;

public class CubeTreeMaker : MonoBehaviour {

	public Material barkMaterial;
	public Material[] leavesMaterials;
	
	SeededRandomiser randomiser;
		
	// Use this for initialization
	void Start () {
		CreateRandomiser();
		//CreateTreesInBox(forest, new Rect(-20, -20, 20, 20), 100, 1f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void CreateRandomiser () {
		if (randomiser == null) randomiser = new SeededRandomiser(12345678);
	}
	
	public CubeTree[] CreateTreesInBox (GameObject holder, Rect rect, int totalTrees, float minimumDistanceBetween) {
		
		if (randomiser == null) CreateRandomiser();
		CubeTree[] trees = new CubeTree[totalTrees];
		int timeoutMax = 100;
		int timeout = timeoutMax;
		int treesMade = 0;
		minimumDistanceBetween *= minimumDistanceBetween;
		while (treesMade < totalTrees && timeout > 0) {
			float xPosition = randomiser.GetRandomFromRange(rect.xMin, rect.xMax);
			float zPosition = randomiser.GetRandomFromRange(rect.yMin, rect.yMax);
			Vector3 pos = new Vector3(xPosition, 0f, zPosition);
			if (IsValidPosition(pos, trees, treesMade, minimumDistanceBetween)) {	
				timeout = timeoutMax;
				CubeTree tree = CreateCubeTree("tree_" + treesMade, pos);
				tree.root.transform.parent = holder.transform;
				tree.root.transform.localEulerAngles = new Vector3(0, randomiser.GetRandomFromRange(-20, 20), 0);
				trees[treesMade++] = tree;
			} else {
				timeout--;
			}
		}
		
		return trees;
	}
	
	bool IsValidPosition (Vector3 position, CubeTree[] trees, int maxTree, float minDist) {
		for (int i = 0; i < maxTree; i++) {
			CubeTree tree = trees[i];
			Vector3 treePos = tree.root.transform.localPosition;
			float xDist = (treePos.x - position.x);
			float zDist = (treePos.z - position.z);
			float dist = (xDist * xDist) + (zDist * zDist);
			
			if (dist < minDist) return false;
		}
		
		return true;
	}
	
	CubeTree CreateCubeTree (string name, Vector3 position) {
		float rnd = randomiser.GetRandom() * leavesMaterials.Length;
		int index = Mathf.FloorToInt(rnd);
		CubeTree tree = new CubeTree(name, barkMaterial, leavesMaterials[index], randomiser, 1f);
		tree.root.transform.localPosition = position;
		
		return tree;
	}
}
