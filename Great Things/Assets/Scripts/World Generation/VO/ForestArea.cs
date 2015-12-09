using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForestArea : MonoBehaviour {

	public Rect rect;
	public string type;
	GameObject ground;
	List<GameObject> forestObjects;
	int jitterCount;
	
	void Update () {
		if (type != null && type.Equals("pool") && ground != null) {
			jitterCount++;
			if (jitterCount > 5) {
				MeshUtils.JitterMeshOnY(ground, -10, 10, 0.001f, -0.1f, 0.1f);
				jitterCount = 0;
			}
		}
	}
	
	public GameObject Ground {
		get { return ground; }
		set {
			ground = value;
			ground.name = "ground";
			ground.transform.parent = transform;
		}
	}
	
	public void AddForestObjects (GameObject objects) {
		if (forestObjects == null) forestObjects = new List<GameObject>();
		forestObjects.Add(objects);
		objects.transform.parent = transform;
	}
	
	public bool Contains (float x, float z) {
		return rect.Contains(new Vector2(x, z));
	}
	
	public bool Overlaps (Rect otherRect) {
		return rect.Overlaps(otherRect);
	}
	
	private bool rendererEnabled;
	public bool RendererEnabled {
		get { return rendererEnabled; }
		set {
			rendererEnabled = value;
		} 
	}
	
	public bool IsInFrontOf (int x, int z, bool rotated, bool flipped) {
		if (!rotated) {
			if (rect.x > x || rect.xMax < x) return false;
			return flipped ? rect.center.y > z : rect.center.y < z;
		} else {
			if (rect.y > z || rect.yMax < z) return false;
			return flipped ? rect.center.x > x : rect.center.x < x;
		}
	}
}
