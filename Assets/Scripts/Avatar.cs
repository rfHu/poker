using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.UI.ProceduralImage;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

// [RequireComponent(typeof(RawImage))]
public class Avatar : MonoBehaviour {

    public Material Opaque;
    public Material Crystal;

	public string Uid;

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

		this.url = url;

		TexturePool.Shared.FetchTexture(url, (texture) => {
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
        if (isInRoom)
        {
            gameObject.GetComponent<RawImage>().material = (Material)Resources.Load("Materials/Mask");
        }
        else
        {
            gameObject.GetComponent<RawImage>().material = (Material)Resources.Load("Materials/Mask.6");
        }
    }
}
