using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.UI.ProceduralImage;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

// [RequireComponent(typeof(RawImage))]
public class Avatar : MonoBehaviour {
	public class TexturePool {
		private Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
		private Dictionary<String, int> counterHash = new Dictionary<string, int>();

		// 最多存100个texture
		private int max = 100;

		private TexturePool() {}

		static public TexturePool Shared = new TexturePool();

		public bool Exists(string url) {
			return textures.ContainsKey(url);
		}

		private void store(string url, Texture2D texture) {
			textures[url] = texture;
		}

		private int insCounter(string url) {
			var count = counterHash.Int(url) + 1;
			counterHash[url] = count;
			return count;
		}

		private int desCounter(string url) {
			var count = counterHash.Int(url) - 1;
			counterHash[url] = count;
			return count;
		}

		public void Despawn(string url) {
			var count = desCounter(url);

			if (textures.Count > max && Exists(url) && count <= 0) {
				var tex = textures[url];
				textures.Remove(url);
				counterHash.Remove(url);
				GameObject.Destroy(tex);

				_.Log("回收Texture");
			}
		} 

		public void Spawn(string url, Action<Texture2D> cb) {
			if (Exists(url)) {
				insCounter(url);
				cb(textures[url]);
			} else {
				_.LoadTexture(url, (texture) => {
					insCounter(url);
					store(url, texture);
					cb(texture);
				});
			}
		}
	}

	public string Uid;
	public Material FullMat;
	public Material AlphaMat;

	public GameObject ClickObject;

	private string url;

	void onClick() {
		if (Uid == null || string.IsNullOrEmpty(Uid)) {
			return ;
		}

		var userPopup = PoolMan.Spawn("User");
        userPopup.GetComponent<DOPopup>().Show();
        userPopup.GetComponent<UserDetail>().Init(Uid);
	}

	public void SetImage(string url) {
		// @FIXME: 后台返回对应的尺寸
        string pattern = @"/w/(\d)+/h/(\d)+/format/jpg";
		Regex rgx = new Regex(pattern);
		url = rgx.Replace(url, "/w/124/h/124/format/png");

		if (!string.IsNullOrEmpty(this.url)) {
			TexturePool.Shared.Despawn(this.url);			
		}

		this.url = url;

		TexturePool.Shared.Spawn(url, (texture) => {
			if (this == null) {
				return ;
			}

			GetComponent<RawImage>().texture = texture;
		});		
	}
	
	void Awake() {
		if (ClickObject == null) {
			ClickObject = gameObject;
		}

		var btn = ClickObject.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
	}

    public void SetAlpha(bool isInRoom) 
    {
		var img = GetComponent<RawImage>();

        if (isInRoom)
        {
            img.material = FullMat;
        }
        else
        {
            img.material = AlphaMat;
        }
    }
}
