using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuPopup : MonoBehaviour {
	public List<Sprite> MuteSprites;
    public GameObject MuteObj;

	public GameObject StandObj;
	public GameObject SuppObj;

	public void Supplement() {
		if (!GameData.MyCmd.Takecoin) {
			return ;
		}
		RxSubjects.TakeCoin.OnNext(new RxData());	
	}

	void Start()
	{
		if (GameData.MyCmd.Unseat) {
			StandObj.GetComponent<CanvasGroup>().alpha = 1;
		}

		if (GameData.MyCmd.Takecoin) {
			SuppObj.GetComponent<CanvasGroup>().alpha = 1;
		}		
	}

	public void Standup() {
		if (!GameData.MyCmd.Unseat) {
			return ;
		}

		Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "unseat"}
		});

		gameObject.GetComponent<DOPopup>().Close();
	}

	public void ToggleMute() {
		if (AudioListener.volume > 0) {
			AudioListener.volume = 0;
			MuteObj.GetComponent<Image>().sprite = MuteSprites[1];
		} else {
			AudioListener.volume = 1;
		    MuteObj.GetComponent<Image>().sprite = MuteSprites[0];
		}
	}

	public void Exit() {
		Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "exit"}
		});
	}

	public void Tips() {
		var tip = (GameObject)Instantiate(Resources.Load("Prefab/CardTip"));
		tip.GetComponent<DOPopup>().Show(G.Cvs);
		gameObject.GetComponent<DOPopup>().ImmediateClose();
	}

	public void Owner() {

	}
}
