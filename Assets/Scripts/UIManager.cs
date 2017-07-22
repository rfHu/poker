using UnityEngine;
using UniRx;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;

public class UIManager : MonoBehaviour {
	private AuditMsg auditMsg;

	public void ShowMenu() {
		var popup = PoolMan.Spawn("MenuPopup");
		popup.GetComponent<DOPopup>().Show();
	}

	public void OnSwipe(Gesture gesture) {
		if (gesture.swipeLength < 200) {
			return ;
		}

		// 有遮罩的情况下不触发手势
		if (ModalHelper.ModalCount > 0) {
			return ;
		}
		
		if (gesture.swipe == EasyTouch.SwipeDirection.Left) {
			OnShowRecalls();
		} else if (gesture.swipe == EasyTouch.SwipeDirection.Right) {
			ScorePage();
		} else if (gesture.swipe == EasyTouch.SwipeDirection.Up && GameData.Shared.MySeat != -1) {
			OnClickExpressionPage();
		}
	}

	public void ScorePage() {
		var score = PoolMan.Spawn("ScorePage");
		score.GetComponent<DOPopup>().Show();	
	}

	public void OnShowRecalls() {
		PoolMan.Spawn("RecallPage");
	}

	public void OnClickChat() {
		Commander.Shared.Chat();
	}

    public void OnClickShareGame() {
        string shareText = ShareGame();
        Commander.Shared.ShareGameRoom(shareText);
    }

    public void OnClickExpressionPage() 
    {
        var expressionPage = PoolMan.Spawn("ExpressionPage");
        expressionPage.GetComponent<DOPopup>().Show();
    }

    public void OnClickShowInsurance()
    {
        Connect.Shared.Emit(new Dictionary<string, object>(){
        	{"f", "showinsurance"}
        });
    }

	void Awake()
	{
		
		RxSubjects.TakeCoin.Subscribe((e) => {
			if (e.Data != null) {
				GameData.Shared.Coins = e.Data.Int("coins");
            }

            //根据类别生成不同预制体
            Transform transform;
            if (GameData.Shared.Type == GameType.Normal)
            {
                transform = PoolMan.Spawn("Supplement");
            }
            else 
            {
                transform = PoolMan.Spawn("JoinMatch");
            }

			
            transform.GetComponent<DOPopup>().Show(() => {
				var player = GameData.Shared.GetMyPlayer();

				if (player.Index == -1 || player.Bankroll.Value > 0) {
					return ;
				}

				Connect.Shared.Emit("unseat");
			});
		}).AddTo(this);

		RxSubjects.Bye.Subscribe((e) => {
			var type = e.Data.Int("type");

			if (type == 2)
            {
				PokerUI.ConflictAlert();
            } else if (type == 3) {
				PokerUI.DisAlert("您已被房主踢出牌局");
			}
		}).AddTo(this);
	}

    void Start() 
    {
        GameData.Shared.ShowAudit.Subscribe((show) =>
        {
            if (!show)
            {
                if (auditMsg != null)
                {
                    auditMsg.GetComponent<DOPopup>().Close();
                    auditMsg = null;
                }

                return;
            }

            auditMsg = PoolMan.Spawn("AuditMsg").GetComponent<AuditMsg>();
            auditMsg.GetComponent<AuditMsg>().Click = () =>
            {
                Commander.Shared.Audit();
            };

            auditMsg.GetComponent<DOPopup>().Show(modal: false, singleton: false);
        }).AddTo(this);	
    }

    public string ShareGame() 
    {
        string str = "";
        str += "\"" + GameData.Shared.Name + "\"邀请您加入\"" + GameData.Shared.RoomName + "\"";

        if (GameData.Shared.Type == GameType.SNG)
        {
            str += "SNG" + GameData.MatchData.MatchString;
        } else if (GameData.Shared.Type == GameType.MTT) {
            str += "MTT" + GameData.MatchData.MatchString;
        }

        if (!string.IsNullOrEmpty(GameData.Shared.GameCode))
            str += "，邀请码[" + GameData.Shared.GameCode + "]";


        if (!GameData.Shared.IsMatch())
        {
            str += "，盲注[";

            if (GameData.Shared.Straddle.Value)
                str += GameData.Shared.SB/2 + "/";

            str += GameData.Shared.SB + "/" + GameData.Shared.BB + "]";

            if (GameData.Shared.Ante.Value > 0)
                str += "，底注[" + GameData.Shared.Ante + "]";           
        }
        
        
        str += "。一键约局，与好友畅享德州扑克的乐趣。";
        return str;
    }
}
