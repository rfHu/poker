﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using MaterialUI;

[RequireComponent(typeof(DOPopup))]
public class RoomMessage : MonoBehaviour {

    public Text OwnerName;
    public Text LeftTime;
    public Text StartTime;
    public Text GameTime;
    public Text AnteMeg;
    public Text SbBb;
    public Text Small;
    public Text Large;
    public Text ThinkTime;
    public Text Insurance;
    public Text Straddle;
    public Text NeedAudit;
    public Text GPSMes;
    public Text IPMes;
    public Text Award27Mes;
    public Text BuryCardMes;
    public Text OmahaLimitRule;

    public GameObject[] Buttons;

    public VectorImage PauseIcon; 
    public VectorImage ContinueIcon;

    void Awake()
    {
        GameData.Shared.LeftTime.Subscribe((value) =>
        {
            if (!GameData.Shared.GameStarted.Value)
            {
                setText(LeftTime, "暂未开始");
                return;
            }
            setText(LeftTime, _.SecondStr(value));
        }).AddTo(this);

         GameData.Shared.Paused.Where((_) => GameData.Shared.GameStarted.Value).Subscribe((pause) => {
            if (pause > 0) {
                PauseIcon.gameObject.SetActive(false);
                ContinueIcon.gameObject.SetActive(true);
            } else {
                PauseIcon.gameObject.SetActive(true);
                ContinueIcon.gameObject.SetActive(false);
            }
        }).AddTo(this);
    }

	public void Init () {
        Insurance.transform.parent.gameObject.SetActive(!((GameData.Shared.Type.Value == GameType.Omaha && GameData.Shared.PlayerCount.Value > 6) || GameData.Shared.Type.Value == GameType.SixPlus));
        Award27Mes.transform.parent.gameObject.SetActive(GameData.Shared.Type.Value == GameType.Normal);
        OmahaLimitRule.transform.parent.gameObject.SetActive(GameData.Shared.Type.Value == GameType.Omaha);

        OwnerName.text = GameData.Shared.OwnerName;

        var cTime = GameData.Shared.CreateTime;
        StartTime.text = cTime.Month + "月" + cTime.Day + "日   " + _.Fix(cTime.Hour) + ":" + _.Fix(cTime.Minute);

        GameTime.text = (float)GameData.Shared.Duration / 3600 + "小时";
        AnteMeg.text = GameData.Shared.Ante.Value.ToString();
        SbBb.text = GameData.Shared.SB + "/" + GameData.Shared.BB;
        Small.text = GameData.Shared.BankrollMul[0] * 100 + "BB";
        Large.text = GameData.Shared.BankrollMul[1] * 100 + "BB";
        ThinkTime.text = GameData.Shared.ThinkTime + "s";

        _.SetMsgText(GameData.Shared.NeedInsurance.Value, Insurance);
        _.SetMsgText(GameData.Shared.Straddle.Value, Straddle);
        _.SetMsgText(GameData.Shared.NeedAudit, NeedAudit);
        _.SetMsgText(GameData.Shared.GPSLimit.Value, GPSMes);
        _.SetMsgText(GameData.Shared.IPLimit.Value, IPMes);
        _.SetMsgText(GameData.Shared.Award27, Award27Mes);
        _.SetMsgText(GameData.Shared.BuryCard, BuryCardMes);
        _.SetMsgText(GameData.Shared.LimitRule, OmahaLimitRule);
        
        foreach (var item in Buttons)
        {
            item.SetActive(GameData.Shared.Owner);
        }
	}

    private void setText(Text go, String text)
    {
        go.text = text;
    }
    public void Stop()
    {
        GetComponent<DOPopup>().Close();

        // 二次确定
        PokerUI.Alert("确定提前结束牌局", () =>
        {
            Connect.Shared.Emit(new Dictionary<string, object>() {
				{"f", "pause"},
				{"args", "3"}
			});
        }, null);
    }

    public void Pause()
    {
        string f;
        int paused;

        if (GameData.Shared.Paused.Value > 0 && GameData.Shared.GameStarted.Value)
        {
            f = "start";
            paused = 1;
        }
        else
        {
            f = "pause";
            paused = 0;
        }

        GameData.Shared.Paused.OnNext(paused);

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", f},
			{"args", "0"}
		}, (data, err) =>
        {
            if (err == 1301) {
                PokerUI.Alert("游戏还未开始");
            }

            GetComponent<DOPopup>().Close();
        });
    }

    public void OwnerPage() 
    {
        GetComponent<DOPopup>().Close();
        var go = PoolMan.Spawn("OwnerPanel");
        go.GetComponent<DOPopup>().Show();
        go.GetComponent<OwnerPanel>().Init();
    }

    public void GamerListPage() 
    {
        GetComponent<DOPopup>().Close();
        var go = PoolMan.Spawn("GamerList");
        go.GetComponent<DOPopup>().Show();
    }
}
