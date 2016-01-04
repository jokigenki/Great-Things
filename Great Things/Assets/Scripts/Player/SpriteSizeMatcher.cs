using UnityEngine;
using System.Collections;

public class SpriteSizeMatcher : MonoBehaviour {

	public SpriteRenderer targetSprite;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		print("width: " + (targetSprite));
	}
}
