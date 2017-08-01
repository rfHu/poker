﻿using UnityEngine;
using System.Linq;
using DarkTonic.MasterAudio;
using PathologicalGames;
using UnityEngine.UI;

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
	public static bool IsReady() {
		return PoolManager.Pools.ContainsKey("Shared");
	}

	public static Transform Spawn(string name) {
		return Spawn(name, null);	
	}

	public static Transform Spawn(string name, Transform parent) {
		var pool = PoolManager.Pools["Shared"];
		Transform prefab = pool.prefabs[name];
		return pool.Spawn(prefab, parent);
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