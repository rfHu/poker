using UnityEngine;
using System.Linq;
using DarkTonic.MasterAudio;
using PathologicalGames;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UniRx;

public class G {
	public static Canvas UICvs {
		get {
			if (uiCanvas == null) {
				var cvs = GameObject.FindGameObjectWithTag("UICanvas");

				if (cvs != null) {
					uiCanvas = cvs.GetComponent<Canvas>();
				}
			}

			return uiCanvas;
		}
	} 

	public static Canvas DialogCvs {
		get {
			if (dialogCanvas == null) {
				dialogCanvas = GameObject.FindGameObjectWithTag("DialogCanvas").GetComponent<Canvas>();
			}

			return dialogCanvas;
		}
	} 

	public static Canvas MaterialCvs {
		get {
			if (materialCanvas == null) {
				materialCanvas = GameObject.FindGameObjectWithTag("MaterialCanvas").GetComponent<Canvas>();
			}

			return materialCanvas;
		}
	}

	private static Canvas uiCanvas;
	private static Canvas dialogCanvas;
	private static Canvas materialCanvas;

	public static void PlaySound(string name) {
		if (GameSetting.muted) {
			return ;
		}

		MasterAudio.PlaySound(name);		
	}

	public static Color Black = new Color(0, 0, 0, 0.9f);

}

public class PoolMan {
	// static private Dictionary<string, GameObject> resources = new Dictionary<string, GameObject>();

	public static bool IsReady() {
		return PoolManager.Pools.ContainsKey("Shared");
	}

	public static Transform Spawn(string name) {
		return Spawn(name, null);	
	}

	public static Transform Spawn(string name, Transform parent) {
		var pool = PoolManager.Pools["Shared"];

		if (!pool.prefabs.ContainsKey(name)) {
			var path = "Prefab/" + name;
			var go = Resources.Load<GameObject>(path);
			go.name = name;
			var prefabPool = new PrefabPool(go.transform);
			pool.CreatePrefabPool(prefabPool);
		}
		
		return pool.Spawn(pool.prefabs[name], parent);
	}

	public static void Despawn(Transform transform) {
		if (!IsSpawned(transform)) {
			return ;
		}

		PoolManager.Pools["Shared"].Despawn(transform);
	}

	public static void Despawn(GameObject go) {
		Despawn(go.transform);
	}

	public static bool IsSpawned(Transform transform) 
	{
		var pool = PoolManager.Pools["Shared"];
		return pool.IsSpawned(transform);
	}

	public static bool Contains(Transform transform) {
		var pool = PoolManager.Pools["Shared"];
		return pool.GetPrefab(transform) != null;
	}

	public static void DespawnAll() {
		if (!PoolManager.Pools.ContainsKey("Shared")) {
			return ;
		}

		var pool  = PoolManager.Pools["Shared"];

		if (pool == null) {
			return ;
		} 

		pool.DespawnAll();
	}
}

public class SceneMan {
	static private string currentScene = "";

	public static void LoadScene(string name) {
		// if (currentScene == name && IsInGame) {
		// 	return ;
		// }

		PoolMan.DespawnAll();
		SceneManager.LoadScene(name);
		currentScene = name;
	}

	public static bool IsInGame {
		get {
			// return SceneManager.GetActiveScene().name == Scenes.PokerGame;
			return currentScene == Scenes.PokerGame;
		}
	}

	public struct Scenes {
		public static string Loading = "GameLoading";
		public static string PokerGame = "PokerGame";
 
	}
}