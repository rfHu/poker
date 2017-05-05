using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Extensions;

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
    public GameObject EmoticonsTeam;
    public GameObject R1;
    public GameObject R2;
    public Button[] EmoticonButtons;
    public Text[] EmoticonPrice;

    RectTransform rectTransform;
    string Uid;

    public void Init(string Uid)
    {
        rectTransform = gameObject.GetComponent<RectTransform>();

        this.Uid = Uid;

        if (GameData.Shared.Owner && Uid != GameData.Shared.Uid)
        {
            ButtonTeam.SetActive(true);
            rectTransform.sizeDelta = new Vector2(825, 892.5f);
        }

        if (Uid == GameData.Shared.Uid || GameData.Shared.MySeat == -1 || GameData.Shared.FindPlayerIndex(Uid) == -1)
        {
            EmoticonsTeam.SetActive(false);
            rectTransform.sizeDelta -= new Vector2(0, 216);
            R1.GetComponent<RectTransform>().localPosition -=  new Vector3(0, 216, 0);
            R2.GetComponent<RectTransform>().localPosition -= new Vector3(0, 216, 0);
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

            var data  = json.Dict("ret");
			var profile = data.Dict("profile").ToObject<ProfileModel>();
			var achieve = data.Dict("achieve").ToObject<AchieveModel>();

            Name.text = profile.name;
            StartCoroutine(_.LoadImage(profile.avatar, (texture) => {
                Avatar.texture = _.Circular(texture);
            }));

            // 金币数
            Coins.text = achieve.coins;
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

            // 动态表情
            var emotion = data.List("emoticon");

            for (int i = 0; i < emotion.Count; i++)
            {
                var dict = emotion[i] as Dictionary<string, object>;
                EmoticonPrice[i].text = "" + dict.Int("coin");
            }
        });
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
        if (GameData.Shared.FindPlayerIndex(Uid) == -1) {
            return;
        }

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
