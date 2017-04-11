using UnityEngine;
using Extensions;
using UniRx;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;

public class UIManager : MonoBehaviour {
	private GameObject auditMsg;

	private DOPopup menuPopup;

	public void ShowMenu() {
		var popup = (GameObject)Instantiate(Resources.Load("Prefab/MenuPopup"));
		menuPopup = popup.GetComponent<DOPopup>();
		menuPopup.Show();	
	}

	public void OnSwipe(Gesture gesture) {
		var go = gesture.pickedUIElement;
		var cvs = go.GetComponentInParent<Canvas>();

		if (cvs == null) {
			return ;
		}

		if (cvs.gameObject.tag != "UICanvas") {
			return ;
		}
		
		if (gesture.swipe == EasyTouch.SwipeDirection.Left) {
			OnShowRecalls();
		} else if (gesture.swipe == EasyTouch.SwipeDirection.Right) {
			ScorePage();
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

	void Awake()
	{
		GameData.Shared.AuditList.AsObservable().Where((list) => list != null).Subscribe((list) => {
			if (!auditMsg)
            {
                auditMsg = (GameObject)Instantiate(Resources.Load("Prefab/AuditMsg"));
            }

            if (list.Count == 0)
            {
                auditMsg.GetComponent<DOPopup>().Close();
            }
            else
            {
                auditMsg.GetComponent<DOPopup>().Show(null, false);
            }
		}).AddTo(this);	

		RxSubjects.TakeCoin.Subscribe((e) => {
			if (e.Data != null) {
				GameData.Shared.Coins = e.Data.Int("coins");
            }

			if (menuPopup != null) {
				menuPopup.Close();
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

        if (GameData.Shared.Straddle)
            str += GameData.Shared.SB/2 + "/";

        str += GameData.Shared.SB + "/" + GameData.Shared.BB + "]";

        if (GameData.Shared.Ante >0)
            str += "，底注[" + GameData.Shared.Ante + "]";
        
        str += "。一键约局，与好友畅享德州扑克的乐趣。";
        return str;
    }
}
