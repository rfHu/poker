using BestHTTP.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;

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
    public Text JackpotTotal;
    public Text Count;
    public GameObject AwardPre;
    public Transform P2GoParent;

    //P3页面
    public Transform P3GoParent;

    //P4页面
    public Transform P4GoParent;
    public GameObject RoomMsgPre;

    public Toggle[] Toggles;

    private Color selectCol = new Color(33 / 255, 41 / 255, 50 / 255);
    private Color openCol = new Color(24 / 255, 1, 1);
    private long timer = 0;
    private RectTransform _rectTransform;

    private int highLightLevel;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

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

        Toggles[0].onValueChanged.AddListener((isOn) => 
        {
            if (!isOn)
                return;

            setGoSize(false);
        });

        Toggles[1].onValueChanged.AddListener((isOn) => 
        {
            if (!isOn)
                return;

            HTTP.Get("/match-award", new Dictionary<string, object> {
                {"match_id", GameData.Shared.MatchID },
            }, (data) =>
            {
                var awardMsg = Json.Decode(data) as Dictionary<string,object>;

                JackpotTotal.text = awardMsg.Int("total").ToString();
                Count.text = awardMsg.Int("count").ToString();

                for (int i = P2GoParent.childCount - 1; i > -1; i--)
                {
                    Destroy(P2GoParent.GetChild(i).gameObject);
                }

                var roomsMsg = awardMsg.List("list");

                setGoSize(roomsMsg.Count > 5);

                for (int i = 0; i < roomsMsg.Count; i++)
                {
                    var msg = roomsMsg[i] as Dictionary<string, object>;

                    GameObject go = Instantiate(AwardPre, P2GoParent);
                    go.SetActive(true);
                    go.transform.GetChild(0).GetComponentInChildren<Text>().text = msg.Int("rank").ToString();
                    go.transform.GetChild(1).GetComponentInChildren<Text>().text = msg.String("award");
                    if ((i + 1) % 2 == 1)
                    {
                        go.AddComponent<ProceduralImage>().color = new Color(0, 0, 0, 0.2f);
                    }
                }
            });
        });


        Toggles[2].onValueChanged.AddListener((isOn) =>
        {
            if (!isOn)
                return;

            setGoSize(true);

            Transform turnNormal = P3GoParent.GetChild(highLightLevel);
            turnNormal.GetChild(1).GetComponent<Text>().color = new Color(1, 1, 1, 0.6f);
            turnNormal.GetChild(2).GetComponent<Text>().color = new Color(1, 1, 1, 0.6f);

            highLightLevel = GameData.Shared.BlindLv;
            Transform highLight = P3GoParent.GetChild(highLightLevel);
            highLight.GetChild(1).GetComponent<Text>().color = selectCol;
            highLight.GetChild(2).GetComponent<Text>().color = selectCol;

        });


        Toggles[3].onValueChanged.AddListener((isOn) => 
        {
            if (!isOn)
                return;
            
            HTTP.Get("/match-rooms", new Dictionary<string, object> {
                {"match_id", GameData.Shared.MatchID },
            }, (data) =>
                {
                    for (int i = P4GoParent.childCount - 1; i > -1; i--)
                    {
                        Destroy(P4GoParent.GetChild(i).gameObject);
                    }

                    var roomsMsg = Json.Decode(data) as List<object>;

                    setGoSize(roomsMsg.Count > 8);

                    for (int i = 0; i < roomsMsg.Count; i++)
                    {
                        var msg = roomsMsg[i] as Dictionary<string, object>;

                        GameObject go = Instantiate(RoomMsgPre, P4GoParent);
                        go.SetActive(true);
                        go.transform.GetChild(0).GetComponentInChildren<Text>().text = msg.Int("num").ToString();
                        go.transform.GetChild(1).GetComponentInChildren<Text>().text = msg.Int("gamers_count").ToString();
                        go.transform.GetChild(2).GetComponentInChildren<Text>().text = msg.Int("min") + "/" + msg.Int("max");
                        if ((i+1)%2 == 1)
                        {
                            go.AddComponent<ProceduralImage>().color = new Color(0, 0, 0, 0.2f);
                        }
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
            TimePassed.text = "已进行：" + G.SecToStr(timer);
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
            timer = roomsData.Long("spent");
            if (timer > 0)
            {
                TimePassed.text = "已进行：" +  G.SecToStr(timer);
            }
            else 
            {
                TimePassed.text = "尚未开始";
            }

            //p1信息
            TableNum.text = roomsData.Int("table_num").ToString();
            RoomPlayerNum.text = roomsData.Int("max_seats").ToString();
            RoomNum.text = roomsData.Int("table_count").ToString();
            Rebuy.text = roomsData.Int("rebuy_count").ToString();
            AddOn.text = roomsData.Int("add_on").ToString();
            CreatorName.text = roomsData.String("creator_name");
            BlindLv.text = roomsData.Int("blind_lv").ToString();
            LimitLevel.text = roomsData.Int("limit_level").ToString();

            G.SetMesText(roomsData.Int("half_break") == 1, HalfBreak);
            G.SetMesText(roomsData.Int("gps_limit") == 1, GPSLimit);
            G.SetMesText(roomsData.Int("ip_limit") == 1, IPLimit);
        });

        Toggles[0].isOn = true;
    }

    private void setGoSize(bool addHeight) 
    {
        if (addHeight && _rectTransform.sizeDelta.y != 1286)
        {
            _rectTransform.DOSizeDelta(new Vector2(860, 1286), 0.3f);
        }
        else if (!addHeight && _rectTransform.sizeDelta.y != 1010) 
        {
            _rectTransform.DOSizeDelta(new Vector2(860, 1010), 0.3f);
        }
    }
}
