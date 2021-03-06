﻿using UnityEngine;
using UniRx;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;

public class UIManager : MonoBehaviour {
	private AuditMsg auditMsg;
	public GameObject RecallPagePrefab;
	public GameObject ScorePagePrefab;
	public GameObject MTTScorePagePrefab;
    public GameObject HoldCardPagePrefab;
	public GameObject MenuPrefab;

	public void ShowMenu() {
		var popup = PoolMan.Spawn(MenuPrefab);
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
        Transform target;

        if (GameData.Shared.Type.Value == GameType.MTT) {
            target = PoolMan.Spawn(MTTScorePagePrefab);
        } else {
            target = PoolMan.Spawn(ScorePagePrefab);
        }

		target.GetComponent<DOPopup>().Show();	
	}

	public void OnShowRecalls() {
		PoolMan.Spawn(RecallPagePrefab);
	}

    public void OnClickHoldCardPage()
    {
        PoolMan.Spawn(HoldCardPagePrefab);
    }

	public void OnClickChat() {
		Commander.Shared.Chat();
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
            if (!GameData.Shared.IsMatch())
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
}
