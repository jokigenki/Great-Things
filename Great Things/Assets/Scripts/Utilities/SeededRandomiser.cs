using UnityEngine;
using System.Collections;

[System.Serializable]
public class SeededRandomiser
{
	public int seed;
	public int counter;
	
	public SeededRandomiser (int seed) {
		this.seed = seed;
		counter = 0;
	}
	
	// get a number between 0 and 1 using the built in counter
	public float GetRandom () {
		return GetRandomForCounter(counter++);
	}
	
	// get a number between 0 and 1 for the given counter 
	public float GetRandomForCounter (int counter) {
		Random.seed = counter * seed;
		return Random.value;
	}
	
	// get a number between min and max using the built in counter
	public float GetRandomFromRange (int min, int max) {
		Random.seed = counter++ * seed;
		return Random.Range(min, max); 
	}
	
	// get a number between min and max using the built in counter
	public float GetRandomFromRange (float min, float max) {
		Random.seed = counter++ * seed;
		return Random.Range(min, max); 
	}
	
	// get a number between min and max using the built in counter
	public int GetRandomIntFromRange (float min, float max) {
		Random.seed = counter++ * seed;
		return Mathf.RoundToInt(Random.Range(min, max)); 
	}
	
	// get a number between min and max for the given counter value
	public float GetRandomFromRangeForCounter (float min, float max, int counter) {
		Random.seed = counter * seed;
		return Random.Range(min, max); 
	}
}