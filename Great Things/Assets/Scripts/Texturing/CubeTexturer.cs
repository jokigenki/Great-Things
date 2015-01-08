using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CubeTexturer : MonoBehaviour
{
	public Rect front = new Rect (0, 0, 0.333f, 0.333f);
	public Rect top = new Rect (0.334f, 0, 0.333f, 0.333f);
	public Rect back = new Rect (0.667f, 0, 0.333f, 0.333f);
	public Rect bottom = new Rect (0, 0.334f, 0.333f, 0.333f);
	public Rect left = new Rect (0.334f, 0.333f, 0.334f, 0.333f);
	public Rect right = new Rect (0.667f, 0.334f, 0.333f, 0.333f);

	void Start ()
	{
			UpdateUVs ();
	}
	
	#if UNITY_EDITOR
	void Update () {
		if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) {
			this.enabled = false;
		} else {
			UpdateUVs();
		}
	}
	#endif
	void UpdateUVs ()
	{
			Rect[] faces = {front, top, back, bottom, left, right};
			int[] uvIndices = { 0, 1, 2, 3, 8, 9, 4, 5, 10, 11, 6, 7, 12, 14, 15, 13, 16, 18, 19, 17, 20, 22, 23, 21 };

			MeshFilter mf = GetComponent<MeshFilter> ();
			Mesh mesh = null;
			if (mf != null)
					mesh = mf.sharedMesh;

			if (mesh == null || mesh.uv.Length != 24) {
					Debug.Log ("Script needs to be attached to built-in cube");
					return;
			}

			Vector2[] uvs = mesh.uv;

			int c = 0;
			foreach (Rect face in faces) {
					uvs [uvIndices [c++]] = new Vector2 (face.xMin, face.yMin);
					uvs [uvIndices [c++]] = new Vector2 (face.xMax, face.yMin);
					uvs [uvIndices [c++]] = new Vector2 (face.xMin, face.yMax);
					uvs [uvIndices [c++]] = new Vector2 (face.xMax, face.yMax);
			}

			mesh.uv = uvs;
	}
}
