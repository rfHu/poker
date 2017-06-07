using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RawImage))]
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
		StartCoroutine(_.LoadImage(url, (texture) => {
			if (this == null) {
				return ;
			}
			
			GetComponent<RawImage>().texture = _.Circular(texture);
		}));
	}
	
	void Awake() {
		if (ClickObject == null) {
			ClickObject = gameObject;
		}

	}

    public void AddClickEvent()
    {
        var btn = ClickObject.AddComponent<Button>();
        btn.onClick.AddListener(onClick);

    }
}
