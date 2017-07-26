using BestHTTP.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MTTMsg : MonoBehaviour {

    //标题内信息
    public Text MatchName;
    public Text PlayerNum;

    //底部
    public Text TimePassed;
    public Text ButtomText;

    //P1页面
    public Text EntryFee;
    public Text LvUpTime;
    public Text InitalScore;
    public Text TableNum;
    public Text RoomPlayerNum;
    public Text RoomNum;
    public Text Rebuy;
    public Text AddOn;
    public Text CreatorName;
    public Text BlindLv;
    public Text LimitLevel;
    public Text HalfBreak;
    public Text GPSLimit;
    public Text IPLimit;

    //P2页面
    public Text JackpotType;

    //P4页面
    public GameObject RoomMsgPre;

    public Toggle[] Toggles;

    private Color selectCol = new Color(33 / 255, 41 / 255, 50 / 255);
    private Color openCol = new Color(24 / 255, 1, 1);
    private long timer = 0;

    void Awake()
    {
        foreach (var item in Toggles)
        {
            item.onValueChanged.AddListener((isOn) => 
            {
                if (isOn)
                {
                    item.transform.Find("Label").GetComponent<Text>().color = selectCol;
                }
                else 
                {
                    item.transform.Find("Label").GetComponent<Text>().color = Color.white - new Color(0, 0, 0, 0.4f);
                }
            });
        }

        Toggles[3].onValueChanged.AddListener((isOn) => 
        {
            if (!isOn)
                return;
            

            HTTP.Get("/match-rooms", new Dictionary<string, object> {
                {"match_id", GameData.Shared.MatchID },
            }, (data) =>
                {
                    var roomsMsg = Json.Decode(data) as List<object>;
                    foreach (var item in roomsMsg)
                    {
                        var msg = item as Dictionary<string, object>;

                        GameObject go = Instantiate(RoomMsgPre);
                        go.transform.GetChild(0).GetComponentInChildren<Text>().text = msg.Int("num").ToString();
                        go.transform.GetChild(1).GetComponentInChildren<Text>().text = msg.Int("gamers_count").ToString();
                        go.transform.GetChild(2).GetComponentInChildren<Text>().text = msg.Int("min") + "/" + msg.Int("max");
                    }
            });
        });

        // 倒计时
        Observable.Interval(TimeSpan.FromSeconds(1)).AsObservable().Subscribe((_) =>
        {
            // 游戏未开始，不需要修改
            if (!GameData.Shared.GameStarted || !gameObject.activeInHierarchy)
            {
                return;
            }

            timer = timer + 1;

        });
    }

    public void Init() 
    {
        //标题信息
        MatchName.text = GameData.Shared.RoomName;
        PlayerNum.text = GameData.Shared.Players.Count + "/" + GameData.Shared.PlayerCount;

        //p1信息
        var matchData = GameData.MatchData.Data;
        EntryFee.text = matchData[0] + "+" + matchData[2];
        LvUpTime.text = matchData[3] + "分钟";
        InitalScore.text = matchData[1].ToString();

        HTTP.Get("/match/" + GameData.Shared.MatchID, new Dictionary<string, object> {
                {"roomid", GameData.Shared.Room},
        }, (data) =>
        {
            var roomsData = Json.Decode(data) as Dictionary<string, object>;

            //底部相关
            ButtomText.text = "延时报名至第" + roomsData.Int("limit_level") + "级别";
            timer = GetTimeStamp() - roomsData.Long("begin_time");
            if (timer > 0)
            {
                TimePassed.text = "已进行：" + secToStr(timer);
            }
            else 
            {
                TimePassed.text = "尚未开始";
            }

            //p1信息
            TableNum.text = roomsData.Int("table_num").ToString();
            RoomPlayerNum.text = roomsData.Int("max_seats").ToString();
            RoomNum.text = "";
            Rebuy.text = roomsData.Int("rebuy_count").ToString();
            AddOn.text = roomsData.Int("add_on").ToString();
            CreatorName.text = roomsData.String("creator_name");
            BlindLv.text = roomsData.Int("blind_lv").ToString();
            LimitLevel.text = roomsData.Int("limit_level").ToString();

            HalfBreak.text = roomsData.Int("half_break") == 1 ? "开启" : "关闭";
            HalfBreak.color = roomsData.Int("half_break") == 1 ? openCol : Color.white;

            GPSLimit.text = roomsData.Int("gps_limit") == 1 ? "开启" : "关闭";
            GPSLimit.color = roomsData.Int("gps_limit") == 1 ? openCol : Color.white;

            IPLimit.text = roomsData.Int("ip_limit") == 1 ? "开启" : "关闭";
            IPLimit.color = roomsData.Int("ip_limit") == 1 ? openCol : Color.white;
        });
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

    private long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds);
    } 
}
