using UnityEngine;
using System.Collections;

public class ForestArea : MonoBehaviour {

	public Rect rect;
	GameObject ground;
	ArrayList forestObjects;
	
	public GameObject Ground {
		get { return ground; }
		set {
			ground = value;
			ground.name = "ground";
			ground.transform.parent = transform;
		}
	}
	
	public void AddForestObjects (GameObject objects) {
		if (forestObjects == null) forestObjects = new ArrayList();
		forestObjects.Add(objects);
		objects.transform.parent = transform;
	}
	
	public bool Contains (float x, float z) {
		return rect.Contains(new Vector2(x, z));
	}
	
	public bool Overlaps (Rect otherRect) {
		return rect.Overlaps(otherRect);
	}
	
	public void UpdateForPosition (int x, int z, int rotation) {
		bool rotated = rotation == 90 || rotation == 270;
		bool flipped = rotation == 180 || rotation == 270;
		
		UpdateForestObjects(x, z, rotated, flipped);
	}
	
	public void UpdateForestObjects (int x, int z, bool rotated, bool flipped) {

		if (forestObjects == null) return;
		
		foreach (GameObject go in forestObjects) { 
			ForestObjects objects = (ForestObjects)go.GetComponent<ForestObjects>();
			//bool isInFront = IsInFrontOf(x, z, rotated, flipped);
			//objects.FadeAxis = isInFront;
			//objects.FlipObjects( flipped, "x");
			//objects.FlipObjects( flipped, "z");
			
			objects.EnableObjects(!rotated, "x");
			objects.EnableObjects(rotated, "z");
		}
	}
	
	private bool rendererEnabled;
	public bool RendererEnabled {
		get { return rendererEnabled; }
		set {
			rendererEnabled = value;
			foreach (GameObject go in forestObjects) { 
				ForestObjects objects = (ForestObjects)go.GetComponent<ForestObjects>();
				objects.EnableObjects(value, "x");
				objects.EnableObjects(value, "z");
			}
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
