using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using MaterialUI;
using UniRx;

//菜单弹出
public class MenuPopup : MonoBehaviour {

	public CanvasGroup StandCG;
	public CanvasGroup SuppCG;
	
	private float disabled = 0.4f;

	public GameObject HangGo;
	public GameObject ReserveGo;
    public GameObject ExchangeGo;
    public GameObject ShareGo;

    public GameObject[] SNGHideGos;

    public GameObject RebuyAddonGo;

	public Text RebuyOrAddonText;

	public GameObject Explain;


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
        var player = GameData.Shared.GetMyPlayer();

        // （非MTT || 过了增购级别 || 游客）不展示按钮 
        if (GameData.Shared.Type.Value != GameType.MTT 
            || !GameData.MatchData.CanBuyLv() 
            || !player.IsValid()
            || GameData.Shared.TableNumber.Value == 0
        ) {
            RebuyAddonGo.SetActive(false);
            return ;
        }

        RebuyAddonGo.SetActive(true);

        var interactable = true; 

        if (GameData.MatchData.CanRebuyLv())
        {
            RebuyOrAddonText.text = "重购";
            bool lessRoll = player.Bankroll.Value <= GameData.MatchData.BankrollNum;
            interactable = player.CanRebuy && lessRoll; 
        }
        else if (GameData.MatchData.CanAddonLv())
        {
         	RebuyOrAddonText.text = "增购";
            interactable = player.CanAddon;
        }

        RebuyAddonGo.GetComponent<CanvasGroup>().alpha = interactable ? 1 : disabled;
        RebuyAddonGo.GetComponent<Button>().interactable = interactable;
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
        StandCG.alpha = GameData.MyCmd.Unseat ? 1 : disabled;
        SuppCG.alpha = GameData.MyCmd.Takecoin ? 1 : disabled;

        // 可下分  
        if (GameData.Shared.Bankroll.Value > 0 && GameData.Shared.OffScore.Value)
        {
            ExchangeGo.SetActive(true);
        }
        else
        {
            ExchangeGo.SetActive(false);
        }

        // 没有位置则不能托管
        if (GameData.Shared.MySeat < 0)
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

        ShareGo.SetActive(GameData.Shared.LeagueID == "");
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
                hangCvg.alpha = disabled;
                hangBtn.interactable = false;
                break;
            case PlayerState.Reserve: // 留座状态，托管可点
                reserveCvg.alpha = disabled;
                reserveBtn.interactable = false;
                break;
            case PlayerState.Normal: // 正常状态，已复原
                break;
            case PlayerState.Auditing:
            case PlayerState.Waiting: // 未带入记分牌，不可点
                hangCvg.alpha = disabled;
                hangBtn.interactable = false;
                reserveCvg.alpha = disabled;
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

		var player = GameData.Shared.GetMyPlayer();

		if (player.IsValid() && player.Chips > 0 && player.InGame) {
			PokerUI.Alert("站起将直接弃牌，是否继续？", standup, null);
		} else {
			standup();
		}

		gameObject.GetComponent<DOPopup>().Close();
	}

	private void standup() {
		Connect.Shared.Emit(new Dictionary<string, object>(){
			{"f", "unseat"}
		});
	}

    public void Option() 
    {
        var tip = PoolMan.Spawn("Option");
        tip.GetComponent<DOPopup>().Show();
    }

	public void Exit() {
        GetComponent<DOPopup>().Close();

		var player = GameData.Shared.GetMyPlayer();

		if (player.IsValid() && player.InGame && player.Chips > 0) {
			PokerUI.Alert("现在退出将直接弃牌，是否强行退出？", exit, null);
		} else {
			exit();
		}
	}

	private void exit() {
		External.Instance.Exit();
	}

	public void Tips() {
        var tip = PoolMan.Spawn(Explain);
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
		Connect.Shared.Emit(new Dictionary<string, object>{
            {"f", "hang"}
        }, (_) => {
            var player = GameData.Shared.GetMyPlayer();
            if (player.IsValid()) {
                player.SetState((int)PlayerState.Hanging);
            }
        });
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
        if (!GameData.MatchData.CanBuyLv()) {
            return ;
        }

        var go = PoolMan.Spawn("RebuyOrAddon");
        go.GetComponent<DOPopup>().Show();

        var ins = go.GetComponent<RebuyOrAddon>();

        if (GameData.MatchData.CanRebuyLv()) {
            ins.Rebuy();
        } else {
            ins.AddOn();
        }
    }

    public void OnShare()
    {
        string shareText = ShareGame();
        Commander.Shared.ShareGameRoom(shareText);
    }

    private string ShareGame()
    {
        string str = "";
        str += "\"" + GameData.Shared.Name + "\"邀请您加入\"" + GameData.Shared.RoomName + "\"";

        var type = GameData.Shared.Type.Value;

        if (type == GameType.SNG)
        {
            str += "SNG" + GameData.MatchData.MatchString;
        }
        else if (type == GameType.MTT)
        {
            str += "MTT" + GameData.MatchData.MatchString;
        }

        if (!string.IsNullOrEmpty(GameData.Shared.GameCode.Value))
            str += "，邀请码[" + GameData.Shared.GameCode + "]";


        if (!GameData.Shared.IsMatch())
        {
            str += "，盲注[";

            if (GameData.Shared.Straddle.Value)
                str += GameData.Shared.SB / 2 + "/";

            str += GameData.Shared.SB + "/" + GameData.Shared.BB + "]";

            if (GameData.Shared.Ante.Value > 0)
                str += "，前注[" + GameData.Shared.Ante + "]";
        }


        str += "。一键约局，与好友畅享德州扑克的乐趣。";
        return str;
    }
}
