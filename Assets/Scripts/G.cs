﻿using UnityEngine;
using System.Linq;
using DarkTonic.MasterAudio;
using PathologicalGames;

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

	private static Canvas uiCanvas;
	private static Canvas dialogCanvas;

	public static void PlaySound(string name) {
		if (GameData.Shared.muted) {
			return ;
		}

		MasterAudio.PlaySound(name);		
	}

	public static Color Black = new Color(0, 0, 0, 0.9f);
}


public class PoolMan {
	public static Transform Spawn(string name) {
		return Spawn(name, null);	
	}

	public static Transform Spawn(string name, Transform parent) {
		var pool = PoolManager.Pools["Shared"];
		Transform prefab = pool.prefabs[name];
		return pool.Spawn(prefab, parent);
	}

	public static void Despawn(Transform transform) {
		PoolManager.Pools["Shared"].Despawn(transform);
	}
}