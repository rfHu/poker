using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DarkTonic.MasterAudio;
using MaterialUI;
using UniRx;
using Extensions;

//菜单弹出
public class MenuPopup : MonoBehaviour {
    public VectorImage  MuteIcon;

	public CanvasGroup StandCG;
	public CanvasGroup SuppCG;
	
	private float originalVolume;
	private float da = 0.4f;

	public GameObject HangGo;
	public GameObject ReserveGo;

	public void Supplement() {
		if (!GameData.MyCmd.Takecoin) {
			return ;
		}
		RxSubjects.TakeCoin.OnNext(new RxData());	
	}

	void Awake()
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

		// 还未坐下
		if (!GameData.MyCmd.Unseat) {
			HangGo.SetActive(false);
			ReserveGo.SetActive(false);
		}

		GameData.Shared.SelfState.Subscribe((state) => {
			var hangCvg = HangGo.GetComponent<CanvasGroup>();
			var reserveCvg = ReserveGo.GetComponent<CanvasGroup>();
			var hangBtn = HangGo.GetComponent<Button>();
			var reserveBtn = ReserveGo.GetComponent<Button>();
			
			hangCvg.alpha = 1;
			reserveCvg.alpha = 1;
			hangBtn.interactable = true;
			reserveBtn.interactable = true;

			switch(state) {
				case PlayerState.Hanging: 	
					hangCvg.alpha = da;
					hangBtn.interactable = false;
					break;
				case PlayerState.Reserve: // 留座状态，托管可点
					reserveCvg.alpha = da;
					reserveBtn.interactable = false;
					break;
				case PlayerState.Normal: // 正常状态，已复原
					break;
				case PlayerState.Auditing: case PlayerState.Waiting: // 未带入记分牌，不可点
					hangCvg.alpha = da;
					hangBtn.interactable = false;
					reserveCvg.alpha = da;
					reserveBtn.interactable = false;
					break;
				default:
					break;
			}
		}).AddTo(this);
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

	public void OnReserve() {
		Connect.Shared.Emit("reserveseat", success: (json) => {
			var res = json.Dict("ret").Int("will_reserveseat");

			if (res == 1) {
				PokerUI.Toast("这手牌结束后进入留座离状态");
			}
		});
		Close();
	}

	public void OnHang() {
		Connect.Shared.Emit("hang");
		Close();
	}

	public void Close() {
		GetComponent<DOPopup>().Close();
	}

	private void setVolumeImage(string icon) {
		MuteIcon.vectorImageData = MaterialIconHelper.GetIcon(icon).vectorImageData;
	}
}
