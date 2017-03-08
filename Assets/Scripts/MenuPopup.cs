using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DarkTonic.MasterAudio;

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

		if (isMuted) {
			MuteImage.sprite = MuteSprites[1];
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

	private static bool isMuted = false;

	public void ToggleMute() {
		if (isMuted) {
			MasterAudio.UnmuteEverything();
			MuteImage.sprite = MuteSprites[0];	
		} else {
			MasterAudio.MuteEverything();
			MuteImage.sprite = MuteSprites[1];
		}

		isMuted = !isMuted;
	}

	public void Exit() {
		External.Instance.Exit();
	}

	public void Tips() {
		var tip = (GameObject)Instantiate(Resources.Load("Prefab/CardTip"));
		tip.GetComponent<DOPopup>().Show();
		gameObject.GetComponent<DOPopup>().ImmediateClose();
	}
}
