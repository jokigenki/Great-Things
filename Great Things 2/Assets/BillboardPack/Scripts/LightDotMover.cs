using UnityEngine;
using System.Collections;

public class LightDotMover : MonoBehaviour {
	

	// Update is called once per frame
	void Update () {
		renderer.material.SetTextureOffset("_AlphaMap", new Vector2(Mathf.Sin(Time.time * 0.23123f) * 0.5f,Mathf.Sin(Time.time) * 0.5f));
	}
}
