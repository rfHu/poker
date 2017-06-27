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
		var go = gesture.pickedUIElement;

		if (go == null) {
			return ;
		}

		if (gesture.swipeLength < 200) {
			return ;
		}

		var cvs = go.GetComponentInParent<Canvas>();

		if (cvs == null) {
			return ;
		}

		if (cvs.gameObject.tag != "UICanvas") {
			return ;
		}

		// 有遮罩的情况下不触发手势
		var modal = cvs.transform.GetComponentInChildren<ModalHelper>();
		if (modal != null) {
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
		GameData.Shared.ShowAudit.Subscribe((show) => {
			if (!show) {
				if (auditMsg != null) {
					auditMsg.GetComponent<DOPopup>().Close();
					auditMsg = null;
				}

				return;
			}
			
            auditMsg = PoolMan.Spawn("AuditMsg").GetComponent<AuditMsg>();
			auditMsg.GetComponent<AuditMsg>().Click = () => {
				Commander.Shared.Audit();
			};

            auditMsg.GetComponent<DOPopup>().Show(modal: false);
		}).AddTo(this);	

		RxSubjects.TakeCoin.Subscribe((e) => {
			if (e.Data != null) {
				GameData.Shared.Coins = e.Data.Int("coins");
            }
			
            Transform transform = PoolMan.Spawn("Supplement");
            transform.GetComponent<DOPopup>().Show(() => {
				var player = GameData.Shared.GetMyPlayer();

				if (player.Index == -1 || player.Bankroll.Value > 0) {
					return ;
				}

				Connect.Shared.Emit(new Dictionary<string, object>() {
					{"f", "unseat"}
				});
			});
		}).AddTo(this);

		RxSubjects.Bye.Subscribe((e) => {
			var type = e.Data.Int("type");
		
			if (type == 2)
            {
				PokerUI.ConflictAlert();
            }

			if (type == 3) {
				PokerUI.DisAlert("您已被房主踢出牌局");
			}
		}).AddTo(this);
	}

    public string ShareGame() 
    {
        string str = "";
        str += "\"" + GameData.Shared.Name + "\"邀请您加入\"" + GameData.Shared.RoomName + "\"";

        if (!string.IsNullOrEmpty(GameData.Shared.GameCode))
            str += "，邀请码[" + GameData.Shared.GameCode + "]";
        
        str += "，盲注[";

        if (GameData.Shared.Straddle.Value)
            str += GameData.Shared.SB/2 + "/";

        str += GameData.Shared.SB + "/" + GameData.Shared.BB + "]";

        if (GameData.Shared.Ante.Value >0)
            str += "，底注[" + GameData.Shared.Ante + "]";
        
        str += "。一键约局，与好友畅享德州扑克的乐趣。";
        return str;
    }
}
