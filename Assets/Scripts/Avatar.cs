using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button)), RequireComponent(typeof(RawImage))]
public class Avatar : MonoBehaviour {
	public string Uid;

	public Action BeforeClick;

	void onClick() {
		if (Uid == null || string.IsNullOrEmpty(Uid)) {
			return ;
		}

		if (BeforeClick != null) {
			BeforeClick();
		}	

		var userPopup = (GameObject)Instantiate(Resources.Load("Prefab/User"));
		userPopup.GetComponent<DOPopup>().Show(G.Cvs);
		userPopup.GetComponent<UserDetail>().RequestById(Uid);
	}

	public void SetImage(string url) {
		var img = GetComponent<RawImage>();
		_.DownloadImage(img, url);
	}
	
	void Awake() {
		GetComponent<Button>().onClick.AddListener(onClick);
	} 
}
