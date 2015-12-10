using UnityEngine;
using System.Collections;
using UnityEditor;

public class PrefabUtils {

	static float generateCount = 0;
	static float generateTotal = 0;
	static bool generateCancelled = false;
	
	public static void GenerateNestedPrefab(GameObject objectToSave, string meshFolderName, string prefabFolderName) 
	{
		// Create the mesh and prefab folders if they don't exist
		string meshAssetPathRoot = CreateFolderIfMissing("Assets", meshFolderName, false);
		string prefabPathRoot = CreateFolderIfMissing("Assets", prefabFolderName, false);
		
		generateCancelled = false;
		generateCount = 0;
		generateTotal = GetNumberOfObjects(objectToSave);
		
		bool success = ReplaceMeshesWithAssets(objectToSave, meshAssetPathRoot, true);
		
		if (success) {
			UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPathRoot + "/" + objectToSave.name + ".prefab");
			PrefabUtility.ReplacePrefab(objectToSave, prefab);
		}
		
		EditorUtility.ClearProgressBar();
	}
	
	static void UpdateProgress () {
		float current = generateTotal > 0 && generateCount > 0 ? generateCount / generateTotal : 0;
		if (EditorUtility.DisplayCancelableProgressBar("Generating meshes", generateCount + " of " + generateTotal, current)) {
			generateCancelled = true;
		}
	}
	
	public static bool ReplaceMeshesWithAssets(GameObject gObj, string meshPathRoot, bool iterate) {
	
		UpdateProgress();
		
		if (generateCancelled) return false;
		string name = gObj.name;
		if (name == null || name == "") {
			MonoBehaviour.print ("PrefabUtils.GenerateNestedPrefab FAILED! Object to be saved has no name!");
			return false;
		}
		
		bool generateIncremented = false;
		string assetPath = meshPathRoot;
		MeshFilter mf = gObj.GetComponent<MeshFilter>() as MeshFilter;
		int nChildren = gObj.transform.childCount;
		if (nChildren > 0) {
			assetPath = CreateFolderIfMissing(meshPathRoot, name, true);
			generateCount++;
			generateIncremented = true;
		}
		
		if (mf != null) {
			string meshPath = assetPath + "/" + name + ".asset";
			AssetDatabase.CreateAsset(mf.mesh, meshPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			mf.mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
			
			MeshCollider mc = gObj.GetComponent<MeshCollider>() as MeshCollider;
			if (mc != null) {
				mc.sharedMesh = mf.sharedMesh;
			}
			if (!generateIncremented) generateCount++;
		}
		
		if (iterate) {
			foreach (Transform child in gObj.transform)
			{
				bool success = ReplaceMeshesWithAssets(child.gameObject, assetPath, true);
				if (!success || generateCancelled) return false;
			}
		}
		
		return true;
	}
	
	public static int GetNumberOfObjects (GameObject gObj) {
		int i = 0;
		foreach (Transform child in gObj.transform) {
			i++;
			i += GetNumberOfObjects(child.gameObject);
		}
		
		return i;
	}
	
	static string CreateFolderIfMissing (string parentPath, string folderName, bool incrementIfExists) {
		string newFolderPath = parentPath + "/" + folderName;
		if (!AssetDatabase.IsValidFolder(newFolderPath)) {
			AssetDatabase.CreateFolder(parentPath, folderName);	
		} else if (incrementIfExists) {
			string incrementedFolderPath = newFolderPath;
			int i = 0;
			while (AssetDatabase.IsValidFolder(incrementedFolderPath)) {
				i++;
				incrementedFolderPath = newFolderPath + "_" + i;
			}
			AssetDatabase.CreateFolder(parentPath, folderName + "_" + i);
			newFolderPath = incrementedFolderPath;
		}
		
		return newFolderPath;
	}
	
}
