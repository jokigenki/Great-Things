using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class Sublocale : MonoBehaviour {

	public Rect rect;
	public string type;
	GameObject ground;
	GameObject features;
	GameObject shadowCasters;
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
			GameObjectUtility.SetStaticEditorFlags(ground, StaticEditorFlags.LightmapStatic);
		}
	}
	
	public void AddFeatures (SublocaleFeatures sublocFeatures) {
		this.features = sublocFeatures.features;
		this.features.transform.parent = transform;
		
		this.shadowCasters = sublocFeatures.shadowCasters;
		this.shadowCasters.transform.parent = transform;
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
