using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MenuPopup : MonoBehaviour {
	public List<Sprite> MuteSprites;
    public Image MuteImage;

	public CanvasGroup StandCG;
	public CanvasGroup SuppCG;
	
	private float originalVolume;

	public void Supplement() {
		if (!GameData.MyCmd.Takecoin) {
			return ;
		}
		RxSubjects.TakeCoin.OnNext(new RxData());	
	}

	void Start()
	{
		if (GameData.MyCmd.Unseat) {
			StandCG.alpha = 1;
		}

		if (GameData.MyCmd.Takecoin) {
			SuppCG.alpha = 1;
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
			MuteImage.sprite = MuteSprites[1];
		} else {
			AudioListener.volume = 1;
		    MuteImage.sprite = MuteSprites[0];
		}
	}

	public void Exit() {
		External.Instance.Exit();
	}

	public void Tips() {
		var tip = (GameObject)Instantiate(Resources.Load("Prefab/CardTip"));
		tip.GetComponent<DOPopup>().Show(G.Cvs);
		gameObject.GetComponent<DOPopup>().ImmediateClose();
	}
}
