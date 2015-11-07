using UnityEngine;
using System.Collections;

public class ForestObjects : MonoBehaviour {

	GameObject[] xAxis;
	GameObject[] zAxis;
	
	public GameObject xAxisCombined;
	public GameObject zAxisCombined;
	
	public ColourMaterialMap materialMap;
	public SeededRandomiser randomiser;
	
	bool xEnabled = true;
	bool zEnabled = true;
		
	bool rendererEnabled;
	
	
	public bool RendererEnabled {
		get { return rendererEnabled; }
		set {
			if (rendererEnabled == value) return;
			rendererEnabled = value;
			xAxisCombined.GetComponent<Renderer>().enabled = value;
			zAxisCombined.GetComponent<Renderer>().enabled = value;
		}	 
	}
	
	void Update () {
		/*
		if (currentXAlpha != targetXAlpha) {
			if (Mathf.Abs(targetXAlpha - currentXAlpha) < 0.01f) {
				currentXAlpha = targetXAlpha;
			} else {
				currentXAlpha = currentXAlpha + ((targetXAlpha - currentXAlpha) / 4);
			}
			SetAlpha(xAxis, currentXAlpha);
		}
		if (currentZAlpha != targetZAlpha) {
			if (Mathf.Abs(targetZAlpha - currentZAlpha) < 0.01f) {
				currentZAlpha = targetZAlpha;
			} else {
				currentZAlpha = currentZAlpha + ((targetZAlpha - currentZAlpha) / 4);
			}
			SetAlpha(zAxis, currentZAlpha);
		}
		*/
	}
	
	/*
	void SetAlpha (GameObject[] axis, float alpha) {
		foreach (GameObject obj in axis) {
			if (obj == null) continue;
			MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
			renderer.material.SetFloat (alpha);
			Color color = renderer.material.color;
			color.a = alpha;
			renderer.material.color = color;
		}
	}
	*/
	
	public void SetXAxis (GameObject parent, GameObject[] axis) {
		xAxis = axis;
		xAxisCombined = new GameObject();
		xAxisCombined.name = "xAxis";
		xAxisCombined.transform.parent = parent.transform;
		xAxisCombined.AddComponent(typeof(MeshFilter));
		MeshRenderer mr = (MeshRenderer)xAxisCombined.AddComponent(typeof(MeshRenderer));
		mr.material = materialMap.getMaterial(randomiser);
		CombineMeshesForAxis(xAxisCombined, xAxis);
	}
	
	public void SetZAxis (GameObject parent, GameObject[] axis) {
		zAxis = axis;
		zAxisCombined = new GameObject();
		zAxisCombined.name = "zAxis";
		zAxisCombined.transform.parent = parent.transform;
		zAxisCombined.AddComponent(typeof(MeshFilter));
		MeshRenderer mr = (MeshRenderer)zAxisCombined.AddComponent(typeof(MeshRenderer));
		mr.material = materialMap.getMaterial(randomiser);
		CombineMeshesForAxis(zAxisCombined, zAxis);
	}
	
	void CombineMeshesForAxis (GameObject parent, GameObject[] axis) {
		CombineInstance[] combine = new CombineInstance[axis.Length];
		int i = 0;
		while (i < axis.Length) {
			MeshFilter mesh = axis[i].GetComponent<MeshFilter>();
			combine[i].mesh = mesh.sharedMesh;
			combine[i].transform = mesh.transform.localToWorldMatrix;
			Destroy(mesh.gameObject);
			//mesh.gameObject.SetActive(false);
			i++;
		}
		parent.transform.GetComponent<MeshFilter>().mesh = new Mesh();
		parent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
		parent.transform.gameObject.SetActive(true);
	}
	
	public void EnableObjects (bool value, string axisName) {
		GameObject axis = null;
		if (axisName.Equals("z")) {
			if (value == zEnabled) return;
			axis = zAxisCombined;
			zEnabled = value;
		} else {
			if (value == xEnabled) return;
			axis = xAxisCombined;
			xEnabled = value;
		}
		if (axis == null) return;
		MeshRenderer renderer = axis.GetComponent<MeshRenderer>();
		renderer.enabled = value;
	}
	
	/*
	public void EnableObjects (bool value, string axisName) {
		GameObject[] axis = null;
		if (axisName.Equals("z")) {
			if (value == zEnabled) return;
			axis = zAxis;
			zEnabled = value;
		} else {
			if (value == xEnabled) return;
			axis = xAxis;
			xEnabled = value;
		}
		if (axis == null) return;
		foreach (GameObject obj in axis) {
			if (obj == null) continue;
			MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
			renderer.enabled = value;
		}
	}
	
	public void FlipObjects (bool value, string axisName) {
		print ("flip objects:" + value + " axis:" + axisName);
		GameObject[] axis = null;
		if (axisName.Equals("z")) {
			if (value == zFlipped) return;
			axis = zAxis;
			zFlipped = value;
		} else {
			if (value == xFlipped) return;
			axis = xAxis;
			xFlipped = value;
		}
		
		if (axis == null) return;
		foreach (GameObject obj in axis) {
			if (obj == null) continue;
			Vector3 r = obj.transform.localEulerAngles;
			r.y += 180;
			obj.transform.localEulerAngles = r;
		}
	}
	
	/*
	public void FadeToAlpha (float alpha) {
		targetXAlpha = alpha;
		targetZAlpha = alpha;
	}
	*/
}
