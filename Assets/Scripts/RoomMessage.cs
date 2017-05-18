using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using UnityEngine.UI;
using UniRx;
using System;

public class RoomMessage : MonoBehaviour {

    public Text PauseText;
    private string pauseStr = "暂停牌局";
    private string continueStr = "继续牌局";

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

    public GameObject Buttons;

	void Awake () {
        GameData.Shared.Paused.Subscribe((pause) =>
        {
            if (pause && GameData.Shared.GameStarted)
            {
                PauseText.text = continueStr;
            }
            else
            {
                PauseText.text = pauseStr;
            }
        }).AddTo(this);

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
        if (GameData.Shared.NeedInsurance)
        { 
            Insurance.text = "启动";
            Insurance.color = new Color(0.09375f, 1, 1);
        }
        if (GameData.Shared.Straddle.Value)
        {
            Straddle.text = "启动";
            Straddle.color = new Color(0.09375f, 1, 1);
        }
        if (GameData.Shared.NeedAudit)
        {
            NeedAudit.text = "启动";
            NeedAudit.color = new Color(0.09375f, 1, 1);
        }

        if (GameData.Shared.GPSLimit)
        {
            GPSMes.text = "启动";
            GPSMes.color = new Color(0.09375f, 1, 1);
        }

        if (GameData.Shared.IPLimit)
        {
            IPMes.text = "启动";
            IPMes.color = new Color(0.09375f, 1, 1);
        }

        if (GameData.Shared.Owner)
        {
            Buttons.SetActive(true);
            GetComponent<RectTransform>().sizeDelta += new Vector2(0, 128);
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

        if (GameData.Shared.Paused.Value && GameData.Shared.GameStarted)
        {
            f = "start";
        }
        else
        {
            f = "pause";
        }

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", f},
			{"args", "0"}
		}, (data) =>
        {
            var err = data.Int("err");

            if (err == 0)
            {
                if (f == "start")
                {
                    PauseText.text = pauseStr;
                }
                else
                {
                    PauseText.text = continueStr;
                }
            }
            else
            {
                var msg = data.String("ret");
                PokerUI.Alert(msg);
            }
            GetComponent<DOPopup>().Close();
        });
    }

    public void OwnerPage() 
    {
        GetComponent<DOPopup>().Close();
        var go = (GameObject)Instantiate(Resources.Load("Prefab/OwnerPanel"));
        go.GetComponent<DOPopup>().Show();
    }
}
