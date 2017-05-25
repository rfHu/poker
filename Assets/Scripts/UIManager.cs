using UnityEngine;
using Extensions;
using UniRx;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;

public class UIManager : MonoBehaviour {
	private GameObject auditMsg;

	public void ShowMenu() {
		var popup = (GameObject)Instantiate(Resources.Load("Prefab/MenuPopup"));
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
		var score = (GameObject)Instantiate(Resources.Load("Prefab/ScorePage"));
		score.GetComponent<DOPopup>().Show();	
	}
	
	public void OnShowRecalls() {
		var recall = (GameObject)Instantiate(Resources.Load("Prefab/RecallPage"));		
		recall.GetComponent<DOPopup>().Show();
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
        var expressionPage = (GameObject)Instantiate(Resources.Load("Prefab/ExpressionPage"));
        expressionPage.GetComponent<DOPopup>().Show();
    }

	void Awake()
	{
		GameData.Shared.AuditList.AsObservable().Where((list) => list != null).Subscribe((list) => {
			if (!auditMsg)
            {
                auditMsg = (GameObject)Instantiate(Resources.Load("Prefab/AuditMsg"));
				auditMsg.GetComponent<AuditMsg>().Click = () => {
					Commander.Shared.Audit();
				};
            }

            if (list.Count == 0)
            {
                auditMsg.GetComponent<DOPopup>().Close();
            }
            else
            {
                auditMsg.GetComponent<DOPopup>().Show(null, false, false);
            }
		}).AddTo(this);	

		RxSubjects.TakeCoin.Subscribe((e) => {
			if (e.Data != null) {
				GameData.Shared.Coins = e.Data.Int("coins");
            }
			
            GameObject obj = (GameObject)Instantiate(Resources.Load("Prefab/Supplement"));
            obj.GetComponent<DOPopup>().Show(() => {
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
