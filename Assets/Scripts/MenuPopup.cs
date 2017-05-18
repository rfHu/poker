using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DarkTonic.MasterAudio;
using MaterialUI;

//菜单弹出
public class MenuPopup : MonoBehaviour {
    public VectorImage  MuteIcon;

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

		if (GameData.Shared.muted) {
			setVolumeImage("volume_off");
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
		if (GameData.Shared.muted) {
			MasterAudio.UnmuteEverything();
			setVolumeImage("volume_up");	
		} else {
			MasterAudio.MuteEverything();
			setVolumeImage("volume_off");
		}

		GameData.Shared.muted = !GameData.Shared.muted;
	}

	public void Exit() {
		External.Instance.Exit();
	}

	public void Tips() {
        var tip = (GameObject)Instantiate(Resources.Load("Prefab/Explain"));
		tip.GetComponent<DOPopup>().Show();
		gameObject.GetComponent<DOPopup>().ImmediateClose();
	}

	private void setVolumeImage(string icon) {
		MuteIcon.vectorImageData = MaterialIconHelper.GetIcon(icon).vectorImageData;
	}
}
