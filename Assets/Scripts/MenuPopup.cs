using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuPopup : MonoBehaviour {
	public List<Sprite> MuteSprites;
    public GameObject MuteObj;

	private Canvas parent;

	public GameObject StandObj;
	public GameObject SuppObj;

	public void Supplement() {
		if (!GConf.MyCmd.Takecoin) {
			return ;
		}
		Delegates.shared.OnTakeCoin(new DelegateArgs());	
	}

	void Start()
	{
		parent = GameObject.FindGameObjectWithTag("Canvas2").GetComponent<Canvas>();

		if (GConf.MyCmd.Unseat) {
			StandObj.GetComponent<CanvasGroup>().alpha = 1;
		}

		if (GConf.MyCmd.Takecoin) {
			SuppObj.GetComponent<CanvasGroup>().alpha = 1;
		}		
	}

	public void Standup() {
		if (!GConf.MyCmd.Unseat) {
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
			MuteObj.GetComponent<Image>().sprite = MuteSprites[1];
		} else {
			AudioListener.volume = 1;
		    MuteObj.GetComponent<Image>().sprite = MuteSprites[0];
		}
	}

	public void Exit() {

	}

	public void Tips() {
		var tip = (GameObject)Instantiate(Resources.Load("Prefab/CardTip"));
		tip.GetComponent<DOPopup>().Show(parent);
		gameObject.GetComponent<DOPopup>().ImmediateClose();
	}

	public void Owner() {

	}
}
