using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Unity.Linq;

public class BackgroundSprite : MonoBehaviour {
	private static GameObject prefab;

	void Start()
	{
		GameSetting.TableSprite.Subscribe((type) => {
			var name = type == 0 ? "TableBlue" : "TableGreen";

			if (prefab != null && prefab.name == name) {
				fillTable();
				return ;	
			} 

			// 释放内存
			// if (prefab != null) {
			// 	var image = prefab.GetComponent<Image>();
			// 	Resources.UnloadAsset(image.sprite);
			// }

			var prefabPath = string.Format("Prefab/{0}", name);
			prefab = Resources.Load<GameObject>(prefabPath);	
			prefab.name = name;	
		
			fillTable();
		}).AddTo(this);
	}	

	private void fillTable() {
		gameObject.Children().Destroy();
		Instantiate(prefab, transform, false);
	}
}
