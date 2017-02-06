using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button)), RequireComponent(typeof(RawImage))]
public class Avatar : MonoBehaviour {
	public string Uid;

	public Action BeforeClick;

	void onClick() {
		if (Uid == null) {
			return ;
		}

		if (BeforeClick != null) {
			BeforeClick();
		}	

		var userPopup = (GameObject)Instantiate(Resources.Load("Prefab/User"));
		var canvas = GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>();
		userPopup.GetComponent<DOPopup>().Show(canvas);

		userPopup.GetComponent<UserDetail>().RequestById(Uid);
	}

	public void SetImage(string url) {
		var img = GetComponent<RawImage>();
		StartCoroutine(_.DownloadImage(img, url));
	}
	
	void Awake() {
		GetComponent<Button>().onClick.AddListener(onClick);
	} 
}
