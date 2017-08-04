using BestHTTP.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using DG.Tweening;

namespace MTTMsgPage
{

    public class MTTMsg : MonoBehaviour
    {

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
        private int timer = 0;
        private RectTransform _rectTransform;

        private int highLightLevel;

        private IDisposable disposable;

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

                SetGoSize(false);
            });

            Toggles[1].onValueChanged.AddListener((isOn) =>
            {
                if (!isOn)
                    return;

                P2GoParent.GetComponent<MTTMsgP2>().requestData();
            });


            Toggles[2].onValueChanged.AddListener((isOn) =>
            {
                if (!isOn)
                    return;

                SetGoSize(true);

                Transform turnNormal = P3GoParent.GetChild(highLightLevel);
                turnNormal.GetChild(1).GetComponentInChildren<Text>().color = new Color(1, 1, 1, 0.6f);
                turnNormal.GetChild(2).GetComponentInChildren<Text>().color = new Color(1, 1, 1, 0.6f);

                highLightLevel = GameData.Shared.BlindLv;
                Transform highLight = P3GoParent.GetChild(highLightLevel);
                highLight.GetChild(1).GetComponentInChildren<Text>().color = openCol;
                highLight.GetChild(2).GetComponentInChildren<Text>().color = openCol;

            });


            Toggles[3].onValueChanged.AddListener((isOn) =>
            {
                if (!isOn)
                    return;

                P4GoParent.GetComponent<MTTMsgP4>().requestData();
            });

            disposable = Observable.Interval(TimeSpan.FromSeconds(1)).AsObservable().Subscribe((__) =>
            {
                if (!gameObject.activeSelf)
                {
                    return;
                }

                timer = timer + 1;
                TimePassed.text = "已进行：" + _.SecondStr(timer);
            });
        }

        void OnDespawned() {
            disposable.Dispose();
        }

        public void Init()
        {
            //标题信息
            MatchName.text = GameData.Shared.RoomName.Value;
            PlayerNum.text = GameData.Shared.Players.Count + "/" + GameData.Shared.PlayerCount;

            //p1信息
            var matchData = GameData.MatchData.Data;
            EntryFee.text = matchData[0] + "+" + matchData[2];
            LvUpTime.text = matchData[3] + "分钟";
            InitalScore.text = matchData[1].ToString();

        HTTP.Get("/match/" + GameData.Shared.MatchID, new Dictionary<string, object> {
                {"roomid", GameData.Shared.Room.Value},
        }, (data) =>
            {
                var roomsData = Json.Decode(data) as Dictionary<string, object>;
                PlayerNum.text = roomsData.Int("valid_gamers") + "/" + roomsData.Int("ready_gamers");

                //底部相关
                ButtomText.text =  roomsData.Int("blind_lv") <= roomsData.Int("limit_level")? "延时报名至第" + roomsData.Int("limit_level") + "级别" : "以截止报名";
                timer = roomsData.Int("spent");
                if (timer > 0)
                {
                    TimePassed.text = "已进行：" + _.SecondStr(timer);
                }
                else
                {
                    TimePassed.text = "尚未开始";
                }

                //p1信息
                TableNum.text = roomsData.Int("table_num") == 0 ? "决赛桌" : roomsData.Int("table_num").ToString();
                RoomPlayerNum.text = roomsData.Int("max_seats").ToString();
                RoomNum.text = roomsData.Int("table_count").ToString();
                Rebuy.text = roomsData.Int("rebuy_count").ToString();
                AddOn.text = roomsData.Int("add_on").ToString();
                CreatorName.text = roomsData.String("creator_name");
                BlindLv.text = roomsData.Int("blind_lv").ToString();
                LimitLevel.text = roomsData.Int("limit_level").ToString();

                _.SetMsgText(roomsData.Int("half_break") == 1, HalfBreak);
                _.SetMsgText(roomsData.Int("gps_limit") == 1, GPSLimit);
                _.SetMsgText(roomsData.Int("ip_limit") == 1, IPLimit);
            });

            Toggles[0].isOn = true;
        }

        public void SetGoSize(bool addHeight)
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
}
