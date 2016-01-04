using UnityEngine;
using System.Collections;

[System.Serializable]
public class ColourMaterialMap {

	public string name;
	public Color[] colors;
	public Material[] materials;
	
	public Material getMaterial (SeededRandomiser rnd) {
		if (materials.Length == 0) return null;
		int index = rnd.GetRandomIntFromRange(0, materials.Length - 1);
		return materials[index];
	}
}
