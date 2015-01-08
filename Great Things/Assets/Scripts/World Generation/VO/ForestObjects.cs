using UnityEngine;
using System.Collections;

public class ForestObjects : MonoBehaviour {

	public GameObject[] xAxis;
	public GameObject[] zAxis;
	
	bool axisEnabled = true;
	bool xEnabled = true;
	bool zEnabled = true;
	bool xFlipped = false;
	bool zFlipped = false;
	float currentXAlpha = 1f;
	float currentZAlpha = 1f;
	float targetXAlpha = 1f;
	float targetZAlpha = 1f;
	
	void Update () {
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
	}
	
	void SetAlpha (GameObject[] axis, float alpha) {
		foreach (GameObject obj in axis) {
			if (obj == null) continue;
			MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
			Color color = renderer.material.color;
			color.a = alpha;
			renderer.material.color = color;
		}
	}
	
	public bool AxisEnabled {
		get { return axisEnabled; }
		set {
			axisEnabled = value;
			FadeToAlpha(value ? 1 : 0.75f);
		}
	}
	
	public GameObject[] XAxis {
		get { return xAxis; }
		set {
			xAxis = value;
		}
	}
	
	public GameObject[] ZAxis {
		get { return zAxis; }
		set {
			zAxis = value;
		}
	}
	
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
	
	public void FadeToAlpha (float alpha) {
		targetXAlpha = alpha;
		targetZAlpha = alpha;
	}
}
