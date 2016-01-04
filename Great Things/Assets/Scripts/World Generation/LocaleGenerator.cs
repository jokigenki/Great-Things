using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public abstract class LocaleGenerator : MonoBehaviour {
	
	public List<ColourMaterialMap> materialMaps;
	public List<Color> poolLightColours;
	public List<Color> forestLightColours;
	
	public GameObject player;
	public Texture2D pathMap;
	public int drawDistance = 10;
	public bool saveToPrefabOnComplete = false;
	public string parentObjectName;
	
	internal Locale locale;
	
	internal Queue<Func<IEnumerator>> generationQueue = new Queue<Func<IEnumerator>>();
	
	// Use this for initialization
	internal void Start () {
		Setup();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	internal virtual void Setup () {
		SeededRandomiser randomiser = new SeededRandomiser(12345678);
		Map map = CreateMap(randomiser);
		CreateParentObjects(map, randomiser);
		
		PopulateGenerationQueue();
		StartCoroutine(StartGeneratorQueue());
	}
	
	internal abstract void PopulateGenerationQueue ();
	
	internal virtual IEnumerator StartGeneratorQueue () {
		while(true)
		{
			if(generationQueue.Count > 0)
			{
				yield return StartCoroutine(generationQueue.Dequeue()());
			}
			else yield return null;
		}
	}
	
	internal virtual ColourMaterialMap GetMaterialMapForName (string name) {
		foreach (ColourMaterialMap map in materialMaps) {
			if (map.name == name) return map;
		}
		
		return null;
	}
	
	
	internal virtual void CreateParentObjects (Map map, SeededRandomiser randomiser) {
		GameObject parentGO = new GameObject ();
		parentGO.name = parentObjectName;
		parentGO.transform.parent = transform;
		
		locale = parentGO.AddComponent(typeof(Locale)) as Locale;
		locale.map = map;
		locale.drawDistance = drawDistance;
		locale.SetRandomiser(randomiser);
		locale.PositionPlayer(player);
		
		Vector3 pos = Vector3.zero;
		pos.y = 0.5f;
		parentGO.transform.localPosition = pos;
		
		locale.sublocales = new List<GameObject>();
	}
	
	internal abstract Map CreateMap (SeededRandomiser randomiser);
}
