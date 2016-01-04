using UnityEngine;
using System.Collections;

public class ColourUtils {
	
	public static bool Match (Color c1, Color c2) {
		return CompareColourValues(c1.r, c2.r, 1000) &&
				CompareColourValues(c1.g, c2.g, 1000) &&
				CompareColourValues(c1.b, c2.b, 1000) &&
				CompareColourValues(c1.a, c2.a, 1000);
	}
	
	public static bool CompareColourValues (float c1, float c2, int factor) {
		return Mathf.RoundToInt(c1 * factor) == Mathf.RoundToInt(c2 * factor);
	}
}