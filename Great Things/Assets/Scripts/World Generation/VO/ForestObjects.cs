using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForestObjects : MonoBehaviour {

	public ColourMaterialMap materialMap;
	public SeededRandomiser randomiser;
	
	bool rendererEnabled;
	
	
	public bool RendererEnabled {
		get { return rendererEnabled; }
		set {
			if (rendererEnabled == value) return;
			rendererEnabled = value;
		}	 
	}
	
	void Update () {
	}
}
