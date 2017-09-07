using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Unity.Linq;

public class BackgroundSprite : MonoBehaviour {
	private static GameObject prevGo;

	void Awake()
	{
		GameSetting.TableSprite.Subscribe((type) => {
			var name = type == 0 ? "TableBlue" : "TableGreen";

			if (prevGo != null && prevGo.name == name) {
				setImage();	
				return ;	
			} 

			// 这样切换会导致内存极剧加大，后期考虑AssetBundle
			var prefabPath = string.Format("Prefab/{0}", name);
			var prefab = Resources.Load<GameObject>(prefabPath);	
			prefab.name = name;
			prevGo = Instantiate(prefab);

			setImage();	
		}).AddTo(this);
	}	

	private void setImage() {
		var image = GetComponent<Image>();
		image.sprite = prevGo.GetComponent<TableSpriteKeeper>().ImageSprite;
		image.enabled = true;
	}
}
