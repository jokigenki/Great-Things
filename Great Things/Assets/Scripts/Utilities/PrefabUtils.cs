using UnityEngine;
using System.Collections;
using UnityEditor;

public class PrefabUtils {

	public static void GenerateNestedPrefab(GameObject objectToSave, string meshFolderName, string prefabFolderName) 
	{
		// Create the mesh and prefab folders if they don't exist
		string meshAssetPathRoot = CreateFolderIfMissing("Assets", meshFolderName);
		string prefabPathRoot = CreateFolderIfMissing("Assets", prefabFolderName);
		
		bool success = ReplaceMeshesWithAssets(objectToSave, meshAssetPathRoot, true);
		
		if (success) {
			//UnityEngine.Object prefab = PrefabUtility.CreateEmptyPrefab(prefabPathRoot + "/" + objectToSave.name + ".prefab");
			//PrefabUtility.ReplacePrefab(objectToSave, prefab);
		}
	}
	
	static bool ReplaceMeshesWithAssets(GameObject gObj, string meshPathRoot, bool iterate) {
	
		string name = gObj.name;
		if (name == null || name == "") {
			MonoBehaviour.print ("PrefabUtils.GenerateNestedPrefab FAILED! Object to be saved has no name!");
			return false;
		}
		
		string assetPath = CreateFolderIfMissing(meshPathRoot, name);
		
		MeshFilter mf = gObj.GetComponent<MeshFilter>() as MeshFilter;
		if (mf != null) {
			string meshPath = assetPath + "/" + name + "_mesh";
			//AssetDatabase.CreateAsset(mf.mesh, meshPath);
			//AssetDatabase.SaveAssets();
			//AssetDatabase.Refresh();
			//mf.mesh = AssetDatabase.LoadAssetAtPath(meshPath);
		}
		
		if (iterate) {
			foreach (Transform child in gObj.transform)
			{
				ReplaceMeshesWithAssets(child.gameObject, assetPath, true);
			}
		}
		
		return true;
	}
	
	static string CreateFolderIfMissing (string parentPath, string folderName) {
		string newFolderPath = parentPath + "/" + folderName;
		if (!AssetDatabase.IsValidFolder(newFolderPath)) {
			AssetDatabase.CreateFolder(parentPath, folderName);	
		}
		
		return newFolderPath;
	}
	
}
