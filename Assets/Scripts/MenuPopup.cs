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
    public GameObject ExchangeGo;

    public GameObject[] SNGHideGos;

    public GameObject RebuyAddonGo;

	public void Supplement() {
		if (!GameData.MyCmd.Takecoin) {
			return ;
		}
		RxSubjects.TakeCoin.OnNext(new RxData());	
	}

	void OnSpawned()
	{
        SNGSetting();
        CommonSetting();

        RebuyAddonSetting();
	}

    private void RebuyAddonSetting()
    {
        RebuyAddonGo.SetActive(GameData.Shared.Type == GameType.MTT);
        if (!RebuyAddonGo.activeInHierarchy)
            return;

        var isInteractable = false;

        var limitLv = GameData.MatchData.LimitLv;

        if (GameData.Shared.BlindLv < limitLv)
        {
            RebuyAddonGo.GetComponentInChildren<Text>().text = "重购";
            isInteractable = GameData.Shared.GetMyPlayer().RebuyCount < GameData.MatchData.Rebuy && GameData.Shared.Bankroll.Value < GameData.MatchData.Data[1];
        }
        else if (GameData.Shared.BlindLv == limitLv)
        {
            RebuyAddonGo.GetComponentInChildren<Text>().text = "增购";
            isInteractable = GameData.Shared.GetMyPlayer().AddonCount < GameData.MatchData.Addon;
        }

        isInteractable = isInteractable && GameData.Shared.FindPlayerIndex(GameData.Shared.Uid) != -1;

        RebuyAddonGo.GetComponent<CanvasGroup>().alpha = (isInteractable) ? 1 : 0.5f;
        RebuyAddonGo.GetComponent<Button>().interactable = isInteractable;
    }

    private void SNGSetting()
    {
        var active = !GameData.Shared.IsMatch();

        foreach (var item in SNGHideGos)
        {
            item.SetActive(active);
        }
    }

    private void CommonSetting()
    {
        StandCG.alpha = GameData.MyCmd.Unseat ? 1 : 0.5f;
        SuppCG.alpha = GameData.MyCmd.Takecoin ? 1 : 0.5f;

        // 可下分  
        if (GameData.Shared.Bankroll.Value > 0 && GameData.Shared.OffScore.Value)
        {
            ExchangeGo.SetActive(true);
        }
        else
        {
            ExchangeGo.SetActive(false);
        }

        // 还未坐下
        if (!GameData.MyCmd.Unseat)
        {
            HangGo.SetActive(false);
            ReserveGo.SetActive(false);
        }
        else
        {
            HangGo.SetActive(true);

            if (!GameData.Shared.IsMatch()) {
                ReserveGo.SetActive(true);
            }
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
        GetComponent<DOPopup>().Close();
		External.Instance.Exit();
	}

	public void Tips() {
        var tip = PoolMan.Spawn("Explain");
        tip.GetChild(0).GetChild(1).GetComponent<Toggle>().isOn = GameData.Shared.IsMatch();
        tip.GetChild(0).GetChild(0).gameObject.SetActive(!GameData.Shared.IsMatch());
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

    public void ExchangeMoney() {
        var text = "将提前为您结算牌局，是否继续？";

        PokerUI.Alert(text, () => 
        {
            Connect.Shared.Emit("offscore");
        }, null);

        GetComponent<DOPopup>().Close();
    }

    public void OnRebuyAddon() 
    {
        var go = PoolMan.Spawn("RebuyOrAddon");
        go.GetComponent<DOPopup>().Show(closeOnClick: false);
        go.GetComponent<RebuyOrAddon>().Init(GameData.Shared.BlindLv < GameData.MatchData.LimitLv);
    }
}
