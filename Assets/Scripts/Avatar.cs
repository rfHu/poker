using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.UI.ProceduralImage;
using System.Text.RegularExpressions;
using UnityEngine.UI.Extensions;

// [RequireComponent(typeof(RawImage))]
public class Avatar : MonoBehaviour {
	public string Uid;

	public Action BeforeClick;

	public GameObject ClickObject;

	void onClick() {
		if (Uid == null || string.IsNullOrEmpty(Uid)) {
			return ;
		}

		if (BeforeClick != null) {
			BeforeClick();
		}	

		var userPopup = (GameObject)Instantiate(Resources.Load("Prefab/User"));
        userPopup.GetComponent<DOPopup>().Show();
        userPopup.GetComponent<UserDetail>().Init(Uid);
	}

	public void SetImage(string url) {
		TexturePool.Shared.FetchTexture(url, (texture) => {
			if (this == null) {
				return ;
			}

			var image = GetComponent<UICircle>();
			image.sprite = Sprite.Create(texture, new Rect (0, 0, texture.width, texture.height), new Vector2 (0.5f, 0.5f));
		});		
	}
	
	void Awake() {
		if (ClickObject == null) {
			ClickObject = gameObject;
		}

		var btn = ClickObject.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
	}
}
