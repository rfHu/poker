﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;
using MaterialUI;

[RequireComponent(typeof(DOPopup))]
public class RoomMessage : MonoBehaviour {

    private string open = "开启";
    private Color openColor = new Color(0.09375f, 1, 1);
    string close = "关闭";

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

    public GameObject[] Buttons;

    public VectorImage PauseIcon; 
    public VectorImage ContinueIcon;

	public void Init () {
        OwnerName.text = GameData.Shared.OwnerName;

        GameData.Shared.LeftTime.Subscribe((value) =>
        {
            if (!GameData.Shared.GameStarted)
            {
                setText(LeftTime, "暂未开始");
                return;
            }

            setText(LeftTime, secToStr(value));
        }).AddTo(this);

        var startNum = GameData.Shared.StartTime;

        StartTime.text = startNum.Month + "月" + startNum.Day + "日   " + startNum.Hour + ":" + startNum.Minute;
        GameTime.text = (float)GameData.Shared.Duration / 3600 + "小时";
        AnteMeg.text = GameData.Shared.Ante.Value.ToString();
        SbBb.text = GameData.Shared.SB + "/" + GameData.Shared.BB;
        Small.text = GameData.Shared.BankrollMul[0] * 100 + "BB";
        Large.text = GameData.Shared.BankrollMul[1] * 100 + "BB";
        ThinkTime.text = GameData.Shared.ThinkTime + "s";

        setMesText(GameData.Shared.NeedInsurance, Insurance);
        setMesText(GameData.Shared.Straddle.Value, Straddle);
        setMesText(GameData.Shared.NeedAudit, NeedAudit);
        setMesText(GameData.Shared.GPSLimit, GPSMes);
        setMesText(GameData.Shared.IPLimit, IPMes);
        setMesText(GameData.Shared.Award27, Award27Mes);
        setMesText(GameData.Shared.BuryCard, BuryCardMes);
        
        foreach (var item in Buttons)
        {
            item.SetActive(GameData.Shared.Owner);
        }

        GameData.Shared.Paused.Where((_) => GameData.Shared.GameStarted).Subscribe((pause) => {
            if (pause > 0) {
                PauseIcon.gameObject.SetActive(false);
                ContinueIcon.gameObject.SetActive(true);
            } else {
                PauseIcon.gameObject.SetActive(true);
                ContinueIcon.gameObject.SetActive(false);
            }
        }).AddTo(this);
	}

    private void setMesText(bool isOpen, Text text) 
    {
        if (isOpen)
        {
            text.text = open;
            text.color = openColor;
        }
        else 
        {
            text.text = close;
            text.color = Color.white;
        }
    }

    private void setText(Text go, String text)
    {
        go.text = text;
    }

    private string secToStr(long seconds)
    {
        var hs = 3600;
        var ms = 60;

        var h = Mathf.FloorToInt(seconds / hs);
        var m = Mathf.FloorToInt(seconds % hs / ms);
        var s = (seconds % ms);

        return string.Format("{0}:{1}:{2}", fix(h), fix(m), fix(s));
    }

    private string fix<T>(T num)
    {
        var str = num.ToString();
        if (str.Length < 2)
        {
            return "0" + str;
        }
        return str;
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

        if (GameData.Shared.Paused.Value > 0 && GameData.Shared.GameStarted)
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
