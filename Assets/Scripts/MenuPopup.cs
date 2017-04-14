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

	private static string muteTag = "persist.txt?tag=mute";

	private bool muted {
		get {
			if (ES2.Exists(muteTag)) {
				return ES2.Load<bool>(muteTag);
			}

			return false;
		}

		set {
			ES2.Save(value, muteTag);
		}
	}

	void Start()
	{
		if (GameData.MyCmd.Unseat) {
			StandCG.alpha = 1;
		}

		if (GameData.MyCmd.Takecoin) {
			SuppCG.alpha = 1;
		}	

		if (muted) {
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
		if (muted) {
			MasterAudio.UnmuteEverything();
			setVolumeImage("volume_up");	
		} else {
			MasterAudio.MuteEverything();
			setVolumeImage("volume_off");
		}

		muted = !muted;
	}

	public void Exit() {
		External.Instance.Exit();
	}

	public void Tips() {
		var tip = (GameObject)Instantiate(Resources.Load("Prefab/CardTip"));
		tip.GetComponent<DOPopup>().Show();
		gameObject.GetComponent<DOPopup>().ImmediateClose();
	}

	private void setVolumeImage(string icon) {
		MuteIcon.vectorImageData = MaterialIconHelper.GetIcon(icon).vectorImageData;
	}
}
