using UnityEngine;
using System.Collections;

public class CutoffModifier : MonoBehaviour {
	public float cutoff = 0f;
	void Update () {
		renderer.material.SetFloat("_Cutoff", cutoff);
	}
}
