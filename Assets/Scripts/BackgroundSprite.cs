using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

[RequireComponent(typeof(Image))]
public class BackgroundSprite : MonoBehaviour {
	private static Sprite sprite;
	private Image bgImage;

	void Start()
	{
		bgImage = GetComponent<Image>();

		GameSetting.TableSprite.Subscribe((type) => {
			var spriteName = type == 0 ? "table" : "table_green";

			if (sprite && sprite.name == spriteName) {
				// skip
			} else {
				// 释放内存
				if (sprite != null) {
					Resources.UnloadAsset(sprite);
				}

				sprite = Resources.Load<Sprite>(spriteName);
			}

			bgImage.sprite = sprite;
			bgImage.enabled = true;
		}).AddTo(this);
	}	
}
