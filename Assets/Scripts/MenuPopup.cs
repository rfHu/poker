using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPopup : MonoBehaviour {
	public List<Sprite> muteSprites;
    public GameObject muteObj;

	public void Standup() {
		var mySeat = GConf.MySeat;

		if (mySeat < 0) {
			return ;
		}

		Connect.shared.Emit(new Dictionary<string, object>(){
			{"f", "unseat"}
		});

		gameObject.GetComponent<DOPopup>().Close();
	}

	public void ToggleMute() {
		if (AudioListener.volume > 0) {
			AudioListener.volume = 0;
			muteObj.GetComponent<Image>().sprite = muteSprites[1];
		} else {
			AudioListener.volume = 1;
		    muteObj.GetComponent<Image>().sprite = muteSprites[0];
		}
	}

	public void Exit() {

	}

	public void Tips() {

	}

	public void Owner() {

	}
}
