﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using MaterialUI;
using UniRx;
using System.Collections;

[RequireComponent(typeof(DOPopup))]
public class UserDetail : MonoBehaviour {

	public RawImage Avatar;
	public Text Name;
    public Text CoinsNumber;
    public Text RemarkText;

    public GameObject GameOptionBtn;
    public CButton AddFriend;   
    public GameObject UserRemark;

    public GameObject EmoticonsList;
    public Button[] EmoticonButtons;

    public GameObject NormalPart;
	public Text Hands;
	public Text ShowHand;
	public Text Join;
	public Text JoinWin; 
	public Text Aggressive;
	public Text PreRaise;
	public Text ThreeBet;
	public Text CBet;

    public GameObject MatchPart;
    public Text SNGJoin;
    public Text ReturnPercent;
    public Text WinMatchCount;
    public Text WinMatchPercent;
    public Text SNGBankroll;
    public Text Golden;
    public Text Silver;
    public Text Copper;

    string Uid;
    private string remark;
    bool enterLimit;
    bool seatLimit;
    bool talkLimit;

    void Awake() 
    {
        for (var i = 0; i < EmoticonButtons.Length; i++)
        {
            var local = i;
            var button = EmoticonButtons[i];

            button.OnClickAsObservable().Subscribe((_) =>
            {
                OnEmoticonClick(local + 1);
            }).AddTo(this);
        }
    }

    public void Init(string Uid)
    {
        this.Uid = Uid;

        buttonInit(Uid);

        if (GameData.Shared.IsMatch())
        {
            NormalPart.SetActive(false);
            MatchPart.SetActive(true);
        }
        else
        {
            NormalPart.SetActive(true);
            MatchPart.SetActive(false);
        }

        //动态表情
        if (Uid == GameData.Shared.Uid || GameData.Shared.FindPlayerIndex(Uid) == -1 || GameSetting.emoticonClose)
        {
            EmoticonsList.SetActive(false);
            GetComponent<VerticalLayoutGroup>().padding.bottom = 40;
        } else {
            EmoticonsList.SetActive(true);
            EmoticonsList.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
        }



        RequestById(Uid);
        GetComponent<DOPopup>().Show();
    }

    private void buttonInit(string Uid)
    {
        bool isMyself = Uid == GameData.Shared.Uid;

        var coinGo = CoinsNumber.transform.parent.gameObject;
        coinGo.SetActive(isMyself);
        if (isMyself) {
            CoinsNumber.text = _.Num2CnDigit(GameData.Shared.Coins);
        }

        RemarkText.gameObject.SetActive(!isMyself);

        var parent = GameOptionBtn.transform.parent.parent.gameObject;

        if (isMyself) {
            parent.SetActive(false);
        } else {
            parent.SetActive(true);
            if (GameData.Shared.Owner && !GameData.Shared.IsMatch())
            {
                GameOptionBtn.SetActive(true);
            }
            else 
            {
                GameOptionBtn.SetActive(false);
            }
            AddFriend.gameObject.SetActive(true);
        }
    }

    void RequestById(string id) {
		var d = new Dictionary<string, object>(){
			{"uid", id}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args", d}
		}, (data) =>
        {
			var profile = data.Dict("profile").ToObject<ProfileModel>();
			var achieve = data.Dict("achieve").ToObject<AchieveModel>();

            Name.text = profile.name;
            Avatar.GetComponent<Avatar>().SetImage(profile.avatar);

            //玩家备注
            remark = data.String("remark");
            if (string.IsNullOrEmpty(remark))
            {
                RemarkText.text = "玩家备注";
            }
            else
            {
                RemarkText.text = remark;
            }

            if (GameData.Shared.IsMatch())
	        {
                setMatchText(achieve);
	        }
            else
	        {
		        setNormalText(data, achieve);
	        }


            // 动态表情
            var emoticon = data.List("emoticon");

            foreach (var item in EmoticonButtons)
            {
                item.GetComponent<Button>().interactable = false;
                item.GetComponentInChildren<Text>().text = "-";
            }

            foreach (var item in emoticon)
            {
                var dict = item as Dictionary<string, object>;
                var pid = dict.Int("id") - 1;

                if (GameData.Shared.Coins < dict.Int("coin"))
                    return;
                    
                EmoticonButtons[pid].GetComponent<Button>().interactable = true;
                EmoticonButtons[pid].GetComponentInChildren<Text>().text = dict.Int("coin").ToString();
            }

            AddFriend.interactable = data.Int("is_friend_or_audit") == 0;
        });

	}

    private void setMatchText(AchieveModel achieve)
    {
        SNGJoin.text = achieve.total_match_count.ToString();

        ReturnPercent.text = _.PercentStr(achieve.bankroll_return_percent);

        WinMatchCount.text = achieve.win_match_count.ToString();

        WinMatchPercent.text = _.PercentStr(achieve.win_match_percent);

        SNGBankroll.text = achieve.total_bankroll_return.ToString();

        Golden.text = achieve.golden_cup_count.ToString();

        Silver.text = achieve.silver_cup_count.ToString();

        Copper.text = achieve.copper_cup_count.ToString();
    }

    void setNormalText(Dictionary<string, object> data, AchieveModel achieve)
    {

        // 手数
        Hands.text = achieve.total_hand_count.ToString();
        // 入池率
        Join.text = _.PercentStr(achieve.entry_hand_percent);
        // 摊牌率
        ShowHand.text = _.PercentStr(achieve.showdown_hand_percent);
        // 入池胜率
        JoinWin.text = _.PercentStr(achieve.entry_win_hand_percent);
        // 激进度
        Aggressive.text = achieve.aggressiveness.ToString();
        // 翻前加注
        PreRaise.text = _.PercentStr(achieve.pfr_hand_percent);
        // 再次加注
        ThreeBet.text = _.PercentStr(achieve.t_bet_percent);
        // 持续下注
        CBet.text = _.PercentStr(achieve.c_bet_round_percent);

        enterLimit = data.Int("enter_limit") == 1;
        seatLimit = data.Int("seat_limit") == 1;
        talkLimit = data.Int("talk_limit") == 1;
    }

    public void OnGamerOptionClick() 
    {
        var transform = PoolMan.Spawn("GamerOption");
        transform.GetComponent<DOPopup>().Show();
        transform.GetComponent<GamerOption>().Init(enterLimit, seatLimit, talkLimit, Uid);
    }

    public void OnEmoticonClick(int Pid) 
    {

        var data = new Dictionary<string, object>(){
			        {"uid", Uid},
                    {"pid", Pid}
		        };

        Connect.Shared.Emit(new Dictionary<string, object>() {
			        {"f", "emoticon"},
			        {"args", data}
		        });

        GetComponent<DOPopup>().Close();
    }


    public void OnRemark() {
        var transform = PoolMan.Spawn("UserRemark");
        transform.GetComponent<UserRemark>().Show(Uid, remark);
    }

    public void OnClickAddFriend() 
    {
        HTTP.Post("/handle-friend", new Dictionary<string, object>()
        {
            {"type", 1},
            {"uid", Uid},
        }, (data) =>
        {
            PokerUI.Toast("好友请求已发送");
            AddFriend.interactable = false;
        });


    }
}
