using UnityEngine;
using System.Collections;
using UnityEditor;

public class LeanLoaderDocumentationEditor : Editor {

	[MenuItem ("Help/LeanLoader Documentation")]
	static void openDocumentation()
	{
		#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3
		// Loops through all items in case the user has moved the default installation directory
		string[] guids = AssetDatabase.FindAssets ("LeanLoader", null);
		string documentationPath = "";
		foreach (string guid in guids){
			string path = AssetDatabase.GUIDToAssetPath(guid);
			if(path.IndexOf("classes/LeanLoader.html")>=0){
				documentationPath = path;
				break;
			}
		}
		documentationPath = documentationPath.Substring(documentationPath.IndexOf("/"));
		string browserPath = "file://" + Application.dataPath + documentationPath + "#index";
		Application.OpenURL(browserPath);

		#else
		// assumes the default installation directory
		string documentationPath = "file://"+Application.dataPath + "/LeanLoader/Documentation/classes/LeanLoader.html#index";
		Application.OpenURL(documentationPath);

		#endif
	}

	[MenuItem ("Help/LeanLoader Forum (ask questions)")]
	static void openForum()
	{
		Application.OpenURL("http://forum.unity3d.com/threads/lean-loader-load-from-the-web-or-a-cache.208899/");
	}

	[MenuItem ("Help/Dented Pixel News")]
	static void openDPNews()
	{
		Application.OpenURL("http://dentedpixel.com/category/developer-diary/");
	}

	
}
