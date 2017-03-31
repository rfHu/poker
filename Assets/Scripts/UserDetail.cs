using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;
using System;

[RequireComponent(typeof(DOPopup))]
public class UserDetail : MonoBehaviour {
	public RawImage Avatar;
	public Text Name;
	public Text Coins;
	public Text Hands;
	public Text ShowHand;
	public Text Join;
	public Text JoinWin; 
	public Text Aggressive;
	public Text PreRaise;
	public Text ThreeBet;
	public Text CBet;
    public GameObject ButtonTeam; 
    public Button[] EmoticonButtons;

    RectTransform rectTransform;
    string Uid;

    public void Init(string Uid)
    {
        rectTransform = gameObject.GetComponent<RectTransform>();

        this.Uid = Uid;

        if (GameData.Shared.Owner && Uid != GameData.Shared.Uid)
        {
            ButtonTeam.SetActive(true);
            rectTransform.sizeDelta = new Vector2(550, 595);
        }

        foreach (var button in EmoticonButtons)
        {
            button.onClick.AddListener(delegate()
            {
                for (int i = 0; i < EmoticonButtons.Length; i++)
                {
                    if (button == EmoticonButtons[i])
                    {
                        OnEmoticonClick(i + 1);
                        break;
                    }
                }
            });
        }
    }

	public void RequestById(string id) {
		var d = new Dictionary<string, object>(){
			{"uid", id}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args", d}
		}, (json) =>
        {
            if (gameObject == null)
            {
                return;
            }

            var data = json.Dict("ret");
            var profile = data.Dict("profile");
            var achieve = data.Dict("achieve");

            Name.text = profile.String("name");
            Coins.text = achieve.String("coins");

            // 手数
            Hands.text = achieve.Int("total_hand_count").ToString();

            // 入池率
            Join.text = _.PercentStr(achieve.Float("entry_hand_percent"));

            // 摊牌率
            ShowHand.text = _.PercentStr(achieve.Float("showdown_hand_percent"));

            // 入池胜率
            JoinWin.text = _.PercentStr(achieve.Float("entry_win_hand_percent"));

            // 激进度
            Aggressive.text = achieve.Float("aggressiveness").ToString();

            // 翻前加注
            PreRaise.text = _.PercentStr(achieve.Float("pfr_hand_percent"));

            // 再次加注
            ThreeBet.text = _.PercentStr(achieve.Float("t_bet_percent"));

            // 持续下注
            CBet.text = _.PercentStr(achieve.Float("c_bet_round_percent"));

            // 动态表情
            var emotion = data.List("emoticon");

            foreach (var item in emotion)
            {
                var dict = item as Dictionary<string, object>;
                dict.Int("coin")
            }

            _.DownloadImage(Avatar, profile.String("avatar"));

        });

        //public void OnClickEmoticon
	}

    public void OnKickOut() 
    {
        var data = new Dictionary<string, object>(){
			{"uid", Uid}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "kickout"},
			{"args", data}
		});

        gameObject.GetComponent<DOPopup>().Close();
    }

    public void OnStandUp() 
    {
        var data = new Dictionary<string, object>(){
			{"uid", Uid}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "standup"},
			{"args", data}
		});

        gameObject.GetComponent<DOPopup>().Close();
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

        gameObject.GetComponent<DOPopup>().Close();
    }
}
