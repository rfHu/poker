using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DarkTonic.MasterAudio;
using MaterialUI;
using UniRx;

//菜单弹出
public class MenuPopup : MonoBehaviour {

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

	void OnSpawned()
	{
        StandCG.alpha = GameData.MyCmd.Unseat ? 1 : 0.6f;
        SuppCG.alpha = GameData.MyCmd.Takecoin ? 1 : 0.6f;

        // 还未坐下
        if (!GameData.MyCmd.Unseat)
        {
            HangGo.SetActive(false);
            ReserveGo.SetActive(false);
        }
        else {
            HangGo.SetActive(true);
            ReserveGo.SetActive(true);
        }

        fitState(GameData.Shared.SelfState.Value);
	}

	void Awake()
	{
		GameData.Shared.SelfState.Subscribe((state) => {
            fitState(state);
		}).AddTo(this);		
	}

    private void fitState(PlayerState state)
    {
        var hangCvg = HangGo.GetComponent<CanvasGroup>();
        var reserveCvg = ReserveGo.GetComponent<CanvasGroup>();
        var hangBtn = HangGo.GetComponent<Button>();
        var reserveBtn = ReserveGo.GetComponent<Button>();

        hangCvg.alpha = 1;
        reserveCvg.alpha = 1;
        hangBtn.interactable = true;
        reserveBtn.interactable = true;

        switch (state)
        {
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
            case PlayerState.Auditing:
            case PlayerState.Waiting: // 未带入记分牌，不可点
                hangCvg.alpha = da;
                hangBtn.interactable = false;
                reserveCvg.alpha = da;
                reserveBtn.interactable = false;
                break;
            default:
                break;
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

    public void Option() 
    {
        var tip = PoolMan.Spawn("Option");
        tip.GetComponent<DOPopup>().Show();
    }

	public void Exit() {
		External.Instance.Exit();
	}

	public void Tips() {
        var tip = PoolMan.Spawn("Explain");
		tip.GetComponent<DOPopup>().Show();
	}

	public void OnReserve() {
		Connect.Shared.Emit("reserveseat", success: (json) => {
			var res = json.Int("will_reserveseat");

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
}
